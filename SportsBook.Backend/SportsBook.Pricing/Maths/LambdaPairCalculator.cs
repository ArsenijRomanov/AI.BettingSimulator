using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;
using SportsBook.Pricing.Abstractions;
using SportsBook.Pricing.Constants;
using SportsBook.Pricing.Enums;
using SportsBook.Pricing.Exceptions;
using SportsBook.Pricing.Markets;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Maths;

public sealed class LambdaPairCalculator : ILambdaPairCalculator
{
    private const int MaxIterations = 100;
    private const double Tolerance = 1e-10;

    private const double MinLambda = 1e-6;
    private const double MinShare = 1e-4;
    private const double MaxShare = 1d - MinShare;

    /// <inheritdoc />
    public (double Home, double Away) Calculate(
        PricedMarketWithBase totalPricedMarket,
        PricedMarketWithBase handicapPricedMarket)
    {
        var totalLambda = CalculateTotalLambda(totalPricedMarket);

        return CalculateByTotalLambdaAndHandicap(
            totalLambda,
            handicapPricedMarket);
    }

    /// <inheritdoc />
    public double CalculateTotalLambda(PricedMarketWithBase totalPricedMarket)
    {
        ArgumentNullException.ThrowIfNull(totalPricedMarket);

        if (totalPricedMarket.Type != MarketType.Total)
        {
            throw new PricingException(
                PricingErrorCodes.InvalidSourceMarket,
                "Total market is required.");
        }

        var (targetOverProbability, _) = Demargin(totalPricedMarket);
        var threshold = (int)Math.Floor(totalPricedMarket.Base.Value);

        var lambda = Math.Max(
            MinLambda,
            totalPricedMarket.Base.Value + 0.5d);

        for (var i = 0; i < MaxIterations; i++)
        {
            var overProbability = PoissonProbabilityCalculator
                .GreaterThan(threshold, lambda)
                .Value;

            var functionValue = overProbability - targetOverProbability.Value;

            if (Math.Abs(functionValue) <= Tolerance)
                return lambda;

            var derivative = PoissonProbabilityCalculator
                .Exact(threshold, lambda)
                .Value;

            if (derivative <= 0d)
            {
                throw new PricingException(
                    PricingErrorCodes.LambdaCalculationFailed,
                    "Newton derivative is too small.");
            }

            lambda -= functionValue / derivative;

            if (!double.IsFinite(lambda) || lambda <= 0d)
                lambda = MinLambda;
        }

        throw new PricingException(
            PricingErrorCodes.LambdaCalculationFailed,
            "Total lambda calculation did not converge.");
    }

    /// <inheritdoc />
    public (double Home, double Away) CalculateByTotalLambdaAndHandicap(
        double totalLambda,
        PricedMarketWithBase handicapPricedMarket)
    {
        if (!double.IsFinite(totalLambda))
            throw new ArgumentOutOfRangeException(nameof(totalLambda), "Total lambda must be finite.");

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(totalLambda, 0d);
        ArgumentNullException.ThrowIfNull(handicapPricedMarket);

        if (handicapPricedMarket.Type != MarketType.Handicap)
        {
            throw new PricingException(
                PricingErrorCodes.InvalidSourceMarket,
                "Handicap market is required.");
        }

        var (targetHomeProbability, _) = Demargin(handicapPricedMarket);

        var left = MinShare;
        var right = MaxShare;

        for (var i = 0; i < MaxIterations; i++)
        {
            var middle = (left + right) / 2d;

            var homeLambda = totalLambda * middle;
            var awayLambda = totalLambda * (1d - middle);

            var homeHandicapProbability = CalculateHomeHandicapProbability(
                homeLambda,
                awayLambda,
                handicapPricedMarket.Base);

            var difference = homeHandicapProbability - targetHomeProbability.Value;

            if (Math.Abs(difference) <= Tolerance)
            {
                return (
                    Home: homeLambda,
                    Away: awayLambda);
            }

            if (homeHandicapProbability < targetHomeProbability.Value)
                left = middle;
            else
                right = middle;
        }

        throw new PricingException(
            PricingErrorCodes.LambdaCalculationFailed,
            "Lambda pair calculation did not converge.");
    }

    /// <summary>
    /// Размаржевывает двухисходный рынок.
    /// </summary>
    /// <param name="pricedMarket">Двухисходный рынок с двумя селекшенами.</param>
    /// <returns>Пара размаржеванных вероятностей в порядке селекшенов рынка.</returns>
    private static (Probability First, Probability Second) Demargin(PricedMarketWithBase pricedMarket)
    {
        ArgumentNullException.ThrowIfNull(pricedMarket);

        if (pricedMarket.Selections.Count != 2)
        {
            throw new PricingException(
                PricingErrorCodes.InvalidSourceMarket,
                "Market must contain exactly two selections.");
        }

        var (firstCode, secondCode) = pricedMarket.Type switch
        {
            MarketType.Total => (SelectionCode.Over, SelectionCode.Under),
            MarketType.Handicap => (SelectionCode.Home, SelectionCode.Away),
            _ => throw new PricingException(
                PricingErrorCodes.UnsupportedMarketType,
                "Market type is not supported for demargin.")
        };

        var first = GetSelection(pricedMarket, firstCode);
        var second = GetSelection(pricedMarket, secondCode);

        var firstImpliedProbability = first.Odds.ToProbability().Value;
        var secondImpliedProbability = second.Odds.ToProbability().Value;

        var impliedProbabilitySum = firstImpliedProbability + secondImpliedProbability;

        if (impliedProbabilitySum <= 0d)
        {
            throw new PricingException(
                PricingErrorCodes.InvalidSourceMarket,
                "Market implied probability sum must be greater than zero.");
        }

        return (
            First: new Probability(firstImpliedProbability / impliedProbabilitySum),
            Second: new Probability(secondImpliedProbability / impliedProbabilitySum));
    }

    private static PricedSelection GetSelection(
        PricedMarketWithBase pricedMarket,
        SelectionCode code)
    {
        PricedSelection? result = null;

        foreach (var selection in pricedMarket.Selections)
        {
            if (selection.Code != code)
                continue;

            if (result is not null)
            {
                throw new PricingException(
                    PricingErrorCodes.InvalidSourceMarket,
                    $"Selection {code} is duplicated.");
            }

            result = selection;
        }

        return result ?? throw new PricingException(
            PricingErrorCodes.InvalidSourceMarket,
            $"Selection {code} was not found.");
    }

    /// <summary>
    /// Считает вероятность прохода домашней форы.
    /// </summary>
    /// <param name="homeLambda">Лямбда домашней команды.</param>
    /// <param name="awayLambda">Лямбда гостевой команды.</param>
    /// <param name="handicapBase">База форы относительно Home.</param>
    /// <returns>Вероятность прохода форы Home.</returns>
    private double CalculateHomeHandicapProbability(
        double homeLambda,
        double awayLambda,
        MarketBase handicapBase)
    {
        var threshold = (int)Math.Floor(-handicapBase.Value) + 1;
        var probability = 0d;

        for (var homeScore = 0; homeScore <= PricingMathConstants.MaxScore; homeScore++)
        {
            var homeScoreProbability = PoissonProbabilityCalculator
                .Exact(homeScore, homeLambda)
                .Value;

            for (var awayScore = 0; awayScore <= PricingMathConstants.MaxScore; awayScore++)
            {
                if (homeScore - awayScore < threshold)
                    continue;

                var awayScoreProbability = PoissonProbabilityCalculator
                    .Exact(awayScore, awayLambda)
                    .Value;

                probability += homeScoreProbability * awayScoreProbability;
            }
        }

        return probability;
    }
}