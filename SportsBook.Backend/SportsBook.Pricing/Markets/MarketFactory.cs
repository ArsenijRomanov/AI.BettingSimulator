using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;
using SportsBook.Pricing.Enums;
using SportsBook.Pricing.Exceptions;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Markets;

internal static class MarketFactory
{
    private const double MinOddsProbability = 1e-12;

    public static PricedMarket<PricedSelection> CreateHomeDrawAway(
        Probability home,
        Probability draw,
        Probability away,
        double margin = 0d)
    {
        if (!Probability.SumApproximatelyEqualsOne(home, draw, away))
            throw new ArgumentException("HomeDrawAway probabilities must sum to 1.");

        return new PricedMarket<PricedSelection>(
            MarketType.HomeDrawAway,
            [
                CreateSelection(SelectionCode.Home, home, margin),
                CreateSelection(SelectionCode.Draw, draw, margin),
                CreateSelection(SelectionCode.Away, away, margin)
            ]);
    }

    public static PricedMarketWithBase CreateTotalFromOver(
        MarketBase marketBase,
        Probability over,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.Total,
            marketBase,
            over,
            margin);

    public static PricedMarketWithBase CreateTotalFromUnder(
        MarketBase marketBase,
        Probability under,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.Total,
            marketBase,
            under.Inverse(),
            margin);

    public static PricedMarketWithBase CreateHomeTotalFromOver(
        MarketBase marketBase,
        Probability over,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.HomeTotal,
            marketBase,
            over,
            margin);

    public static PricedMarketWithBase CreateHomeTotalFromUnder(
        MarketBase marketBase,
        Probability under,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.HomeTotal,
            marketBase,
            under.Inverse(),
            margin);

    public static PricedMarketWithBase CreateAwayTotalFromOver(
        MarketBase marketBase,
        Probability over,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.AwayTotal,
            marketBase,
            over,
            margin);

    public static PricedMarketWithBase CreateAwayTotalFromUnder(
        MarketBase marketBase,
        Probability under,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.AwayTotal,
            marketBase,
            under.Inverse(),
            margin);

    public static PricedMarketWithBase CreateHandicapFromHome(
        MarketBase marketBase,
        Probability home,
        double margin = 0d) =>
        CreateHomeAwayMarket(
            marketBase,
            home,
            margin);

    public static PricedMarketWithBase CreateHandicapFromAway(
        MarketBase marketBase,
        Probability away,
        double margin = 0d) =>
        CreateHomeAwayMarket(
            marketBase,
            away.Inverse(),
            margin);

    public static PricedMarket<PricedCorrectScoreSelection> CreateCorrectScore(
        Score score,
        Probability probability,
        double margin = 0d) =>
        new(
            MarketType.CorrectScore,
            [
                new PricedCorrectScoreSelection(
                    score,
                    probability,
                    ToSafeOdds(probability, margin))
            ]);

    private static PricedMarketWithBase CreateOverUnderMarket(
        MarketType type,
        MarketBase marketBase,
        Probability over,
        double margin)
    {
        var under = over.Inverse();

        return new PricedMarketWithBase(
            type,
            marketBase,
            [
                CreateSelection(SelectionCode.Over, over, margin),
                CreateSelection(SelectionCode.Under, under, margin)
            ]);
    }

    private static PricedMarketWithBase CreateHomeAwayMarket(
        MarketBase marketBase,
        Probability home,
        double margin)
    {
        var away = home.Inverse();

        return new PricedMarketWithBase(
            MarketType.Handicap,
            marketBase,
            [
                CreateSelection(SelectionCode.Home, home, margin),
                CreateSelection(SelectionCode.Away, away, margin)
            ]);
    }

    private static PricedSelection CreateSelection(
        SelectionCode code,
        Probability probability,
        double margin) =>
        new(
            code,
            probability,
            ToSafeOdds(probability, margin));

    private static Odds ToSafeOdds(
        Probability probability,
        double margin)
    {
        if (!double.IsFinite(margin))
            throw new ArgumentOutOfRangeException(nameof(margin), "Margin must be finite.");

        ArgumentOutOfRangeException.ThrowIfLessThan(margin, 0d);

        var bookmakerProbability = probability.Value * (1d + margin);

        if (bookmakerProbability >= 1d)
        {
            throw new PricingException(
                PricingErrorCodes.MarginTooHigh,
                "Margin is too high for this probability.");
        }

        bookmakerProbability = Math.Max(
            bookmakerProbability,
            MinOddsProbability);

        return new Odds(1d / bookmakerProbability);
    }
}