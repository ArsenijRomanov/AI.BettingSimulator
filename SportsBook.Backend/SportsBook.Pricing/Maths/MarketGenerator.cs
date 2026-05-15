using SportsBook.Pricing.Abstractions;
using SportsBook.Pricing.Constants;
using SportsBook.Pricing.Markets;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Maths;

public sealed class MarketGenerator
{
    private readonly double[,] _scoreMatrix;

    public double HomeLambda { get; }
    public double AwayLambda { get; }

    public MarketGenerator(
        double homeLambda,
        double awayLambda)
    {

        if (!double.IsFinite(homeLambda))
            throw new ArgumentOutOfRangeException(nameof(homeLambda), "Home lambda must be finite.");

        if (!double.IsFinite(awayLambda))
            throw new ArgumentOutOfRangeException(nameof(awayLambda), "Away lambda must be finite.");

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(homeLambda, 0d);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(awayLambda, 0d);

        HomeLambda = homeLambda;
        AwayLambda = awayLambda;

        _scoreMatrix = BuildScoreMatrix(HomeLambda, AwayLambda);
    }

    public MarketGenerator(
        ILambdaPairCalculator lambdaPairCalculator,
        MarketWithBase totalMarket,
        MarketWithBase handicapMarket)
    {
        ArgumentNullException.ThrowIfNull(lambdaPairCalculator);
        ArgumentNullException.ThrowIfNull(totalMarket);
        ArgumentNullException.ThrowIfNull(handicapMarket);

        var (home, away) = lambdaPairCalculator.Calculate(
            totalMarket,
            handicapMarket);

        HomeLambda = home;
        AwayLambda = away;

        _scoreMatrix = BuildScoreMatrix(HomeLambda, AwayLambda);
    }

    public Market<Selection> GenerateHomeDrawAway()
    {
        var home = 0d;
        var draw = 0d;
        var away = 0d;

        for (var score = 0; score <= PricingMathConstants.MaxScore; score++)
            draw += _scoreMatrix[score, score];

        for (var homeScore = 1; homeScore <= PricingMathConstants.MaxScore; homeScore++)
        {
            for (var awayScore = 0; awayScore < homeScore; awayScore++)
                home += _scoreMatrix[homeScore, awayScore];
        }

        for (var awayScore = 1; awayScore <= PricingMathConstants.MaxScore; awayScore++)
        {
            for (var homeScore = 0; homeScore < awayScore; homeScore++)
                away += _scoreMatrix[homeScore, awayScore];
        }

        return MarketFactory.CreateHomeDrawAway(
            new Probability(home),
            new Probability(draw),
            new Probability(away));
    }

    public MarketWithBase GenerateTotal(MarketBase marketBase)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(marketBase.Value, 0d);

        var maxTotalScore = PricingMathConstants.MaxScore * 2;

        if (marketBase.Value > maxTotalScore)
            throw new ArgumentOutOfRangeException(nameof(marketBase), "Total base exceeds score matrix size.");

        var threshold = (int)Math.Floor(marketBase.Value);
        var under = 0d;

        var maxHomeScore = Math.Min(
            PricingMathConstants.MaxScore,
            threshold);

        for (var homeScore = 0; homeScore <= maxHomeScore; homeScore++)
        {
            var maxAwayScore = Math.Min(
                PricingMathConstants.MaxScore,
                threshold - homeScore);

            for (var awayScore = 0; awayScore <= maxAwayScore; awayScore++)
                under += _scoreMatrix[homeScore, awayScore];
        }

        return MarketFactory.CreateTotalFromUnder(
            marketBase,
            new Probability(under));
    }

    public MarketWithBase GenerateHomeTotal(MarketBase marketBase)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(marketBase.Value, 0d);

        if (marketBase.Value > PricingMathConstants.MaxScore)
            throw new ArgumentOutOfRangeException(nameof(marketBase), "Home total base exceeds score matrix size.");

        var threshold = (int)Math.Floor(marketBase.Value);
        var under = 0d;

        for (var homeScore = 0; homeScore <= threshold; homeScore++)
        {
            for (var awayScore = 0; awayScore <= PricingMathConstants.MaxScore; awayScore++)
                under += _scoreMatrix[homeScore, awayScore];
        }

        return MarketFactory.CreateHomeTotalFromUnder(
            marketBase,
            new Probability(under));
    }

    public MarketWithBase GenerateAwayTotal(MarketBase marketBase)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(marketBase.Value, 0d);

        if (marketBase.Value > PricingMathConstants.MaxScore)
            throw new ArgumentOutOfRangeException(nameof(marketBase), "Away total base exceeds score matrix size.");

        var threshold = (int)Math.Floor(marketBase.Value);
        var under = 0d;

        for (var awayScore = 0; awayScore <= threshold; awayScore++)
        {
            for (var homeScore = 0; homeScore <= PricingMathConstants.MaxScore; homeScore++)
                under += _scoreMatrix[homeScore, awayScore];
        }

        return MarketFactory.CreateAwayTotalFromUnder(
            marketBase,
            new Probability(under));
    }

    public MarketWithBase GenerateHandicap(MarketBase marketBase)
    {
        if (Math.Abs(marketBase.Value) > PricingMathConstants.MaxScore)
            throw new ArgumentOutOfRangeException(nameof(marketBase), "Handicap base exceeds score matrix size.");

        if (marketBase.Value < 0d)
        {
            var threshold = (int)Math.Floor(-marketBase.Value) + 1;
            var home = 0d;

            for (var awayScore = 0; awayScore <= PricingMathConstants.MaxScore; awayScore++)
            {
                var minHomeScore = awayScore + threshold;

                for (var homeScore = minHomeScore; homeScore <= PricingMathConstants.MaxScore; homeScore++)
                    home += _scoreMatrix[homeScore, awayScore];
            }

            return MarketFactory.CreateHandicapFromHome(
                marketBase,
                new Probability(home));
        }

        var homeThreshold = (int)Math.Floor(-marketBase.Value) + 1;
        var away = 0d;

        for (var awayScore = 0; awayScore <= PricingMathConstants.MaxScore; awayScore++)
        {
            var maxHomeScore = awayScore + homeThreshold - 1;

            if (maxHomeScore < 0)
                continue;

            maxHomeScore = Math.Min(maxHomeScore, PricingMathConstants.MaxScore);

            for (var homeScore = 0; homeScore <= maxHomeScore; homeScore++)
                away += _scoreMatrix[homeScore, awayScore];
        }

        return MarketFactory.CreateHandicapFromAway(
            marketBase,
            new Probability(away));
    }

    public Market<CorrectScoreSelection> GenerateCorrectScore(Score score)
    {
        if (score.Home > PricingMathConstants.MaxScore)
            throw new ArgumentOutOfRangeException(nameof(score), "Home score exceeds score matrix size.");

        if (score.Away > PricingMathConstants.MaxScore)
            throw new ArgumentOutOfRangeException(nameof(score), "Away score exceeds score matrix size.");

        return MarketFactory.CreateCorrectScore(
            score,
            new Probability(_scoreMatrix[score.Home, score.Away]));
    }

    private static double[,] BuildScoreMatrix(
        double homeLambda,
        double awayLambda)
    {
        var homeProbabilities = BuildScoreProbabilities(homeLambda);
        var awayProbabilities = BuildScoreProbabilities(awayLambda);

        var matrix = new double[
            PricingMathConstants.MaxScore + 1,
            PricingMathConstants.MaxScore + 1];

        var sum = 0d;

        for (var homeScore = 0; homeScore <= PricingMathConstants.MaxScore; homeScore++)
        {
            for (var awayScore = 0; awayScore <= PricingMathConstants.MaxScore; awayScore++)
            {
                var probability = homeProbabilities[homeScore] * awayProbabilities[awayScore];

                matrix[homeScore, awayScore] = probability;
                sum += probability;
            }
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sum, 0d);

        for (var homeScore = 0; homeScore <= PricingMathConstants.MaxScore; homeScore++)
        {
            for (var awayScore = 0; awayScore <= PricingMathConstants.MaxScore; awayScore++)
                matrix[homeScore, awayScore] /= sum;
        }

        return matrix;
    }

    private static double[] BuildScoreProbabilities(double lambda)
    {
        var probabilities = new double[PricingMathConstants.MaxScore + 1];

        for (var score = 0; score <= PricingMathConstants.MaxScore; score++)
        {
            probabilities[score] = PoissonProbabilityCalculator
                .Exact(score, lambda)
                .Value;
        }

        return probabilities;
    }
}
