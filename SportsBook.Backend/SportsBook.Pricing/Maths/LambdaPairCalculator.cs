using SportsBook.Pricing.Abstractions;
using SportsBook.Pricing.Enums;
using SportsBook.Pricing.Markets;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Maths;

public sealed class LambdaPairCalculator : ILambdaPairCalculator
{
    private const int DefaultMaxScore = 20;
    private const int DefaultMaxIterations = 100;
    private const double DefaultTolerance = 1e-10;

    private const double MinLambda = 1e-6;
    private const double MinShare = 1e-4;
    private const double MaxShare = 1d - MinShare;

    private readonly int _maxScore;
    private readonly int _maxIterations;
    private readonly double _tolerance;

    public LambdaPairCalculator()
        : this(DefaultMaxScore, DefaultMaxIterations, DefaultTolerance)
    {
    }

    public LambdaPairCalculator(
        int maxScore,
        int maxIterations,
        double tolerance)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxScore, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxIterations, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(tolerance, 0d);

        _maxScore = maxScore;
        _maxIterations = maxIterations;
        _tolerance = tolerance;
    }

    /// <inheritdoc />
    public (double Home, double Away) Calculate(
        MarketWithBase totalMarket,
        MarketWithBase handicapMarket)
    {
        var totalLambda = CalculateTotalLambda(totalMarket);

        return CalculateByTotalLambdaAndHandicap(
            totalLambda,
            handicapMarket);
    }

    /// <inheritdoc />
    public double CalculateTotalLambda(MarketWithBase totalMarket)
    {
        ArgumentNullException.ThrowIfNull(totalMarket);

        if (totalMarket.Type != MarketType.Total)
            throw new ArgumentException("Total market is required.", nameof(totalMarket));

        var (targetOverProbability, _) = Demargin(totalMarket);
        var threshold = (int)Math.Floor(totalMarket.Base.Value);

        var lambda = Math.Max(MinLambda, totalMarket.Base.Value + 0.5d);

        for (var i = 0; i < _maxIterations; i++)
        {
            var overProbability = PoissonProbabilityCalculator
                .GreaterThan(threshold, lambda)
                .Value;

            var functionValue = overProbability - targetOverProbability.Value;

            if (Math.Abs(functionValue) <= _tolerance)
                return lambda;

            var derivative = PoissonProbabilityCalculator
                .Exact(threshold, lambda)
                .Value;

            if (derivative <= 0d)
                throw new InvalidOperationException("Newton derivative is too small.");

            lambda -= functionValue / derivative;

            if (!double.IsFinite(lambda) || lambda <= 0d)
                lambda = MinLambda;
        }

        return lambda;
    }

    /// <inheritdoc />
    public (double Home, double Away) CalculateByTotalLambdaAndHandicap(
        double totalLambda,
        MarketWithBase handicapMarket)
    {
        if (!double.IsFinite(totalLambda))
            throw new ArgumentOutOfRangeException(nameof(totalLambda), "Total lambda must be finite.");

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(totalLambda, 0d);
        ArgumentNullException.ThrowIfNull(handicapMarket);

        if (handicapMarket.Type != MarketType.Handicap)
            throw new ArgumentException("Handicap market is required.", nameof(handicapMarket));

        var (targetHomeProbability, _) = Demargin(handicapMarket);

        var left = MinShare;
        var right = MaxShare;
        var middle = 0.5d;

        for (var i = 0; i < _maxIterations; i++)
        {
            middle = (left + right) / 2d;

            var homeLambda = totalLambda * middle;
            var awayLambda = totalLambda * (1d - middle);

            var homeHandicapProbability = CalculateHomeHandicapProbability(
                homeLambda,
                awayLambda,
                handicapMarket.Base);

            var difference = homeHandicapProbability - targetHomeProbability.Value;

            if (Math.Abs(difference) <= _tolerance)
                break;

            if (homeHandicapProbability < targetHomeProbability.Value)
                left = middle;
            else
                right = middle;
        }

        return (
            Home: totalLambda * middle,
            Away: totalLambda * (1d - middle));
    }

    /// <summary>
    /// Размаржевывает двухисходный рынок.
    /// </summary>
    /// <param name="market">Двухисходный рынок с двумя селекшенами.</param>
    /// <returns>Пара размаржеванных вероятностей в порядке селекшенов рынка.</returns>
    private static (Probability First, Probability Second) Demargin(MarketWithBase market)
    {
        ArgumentNullException.ThrowIfNull(market);

        if (market.Selections.Count != 2)
            throw new ArgumentException("Market must contain exactly two selections.", nameof(market));

        var firstImpliedProbability = market.Selections[0].Odds.ToProbability().Value;
        var secondImpliedProbability = market.Selections[1].Odds.ToProbability().Value;

        var impliedProbabilitySum = firstImpliedProbability + secondImpliedProbability;

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(impliedProbabilitySum, 0d);

        return (
            First: new Probability(firstImpliedProbability / impliedProbabilitySum),
            Second: new Probability(secondImpliedProbability / impliedProbabilitySum));
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

        for (var homeScore = 0; homeScore <= _maxScore; homeScore++)
        {
            var homeScoreProbability = PoissonProbabilityCalculator
                .Exact(homeScore, homeLambda)
                .Value;

            for (var awayScore = 0; awayScore <= _maxScore; awayScore++)
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
