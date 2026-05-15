using SportsBook.Pricing.Enums;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Markets;

internal static class MarketFactory
{
    private const double MinOddsProbability = 1e-12;

    public static Market<Selection> CreateHomeDrawAway(
        Probability home,
        Probability draw,
        Probability away)
    {
        if (!Probability.SumApproximatelyEqualsOne(home, draw, away))
            throw new ArgumentException("HomeDrawAway probabilities must sum to 1.");

        return new Market<Selection>(
            MarketType.HomeDrawAway,
            [
                CreateSelection(SelectionCode.Home, home),
                CreateSelection(SelectionCode.Draw, draw),
                CreateSelection(SelectionCode.Away, away)
            ]);
    }

    public static MarketWithBase CreateTotalFromOver(
        MarketBase marketBase,
        Probability over) =>
        CreateOverUnderMarket(
            MarketType.Total,
            marketBase,
            over);

    public static MarketWithBase CreateTotalFromUnder(
        MarketBase marketBase,
        Probability under) =>
        CreateOverUnderMarket(
            MarketType.Total,
            marketBase,
            under.Inverse());

    public static MarketWithBase CreateHomeTotalFromOver(
        MarketBase marketBase,
        Probability over) =>
        CreateOverUnderMarket(
            MarketType.HomeTotal,
            marketBase,
            over);

    public static MarketWithBase CreateHomeTotalFromUnder(
        MarketBase marketBase,
        Probability under) =>
        CreateOverUnderMarket(
            MarketType.HomeTotal,
            marketBase,
            under.Inverse());

    public static MarketWithBase CreateAwayTotalFromOver(
        MarketBase marketBase,
        Probability over) =>
        CreateOverUnderMarket(
            MarketType.AwayTotal,
            marketBase,
            over);

    public static MarketWithBase CreateAwayTotalFromUnder(
        MarketBase marketBase,
        Probability under) =>
        CreateOverUnderMarket(
            MarketType.AwayTotal,
            marketBase,
            under.Inverse());

    public static MarketWithBase CreateHandicapFromHome(
        MarketBase marketBase,
        Probability home) =>
        CreateHomeAwayMarket(
            marketBase,
            home);

    public static MarketWithBase CreateHandicapFromAway(
        MarketBase marketBase,
        Probability away) =>
        CreateHomeAwayMarket(
            marketBase,
            away.Inverse());

    public static Market<CorrectScoreSelection> CreateCorrectScore(
        Score score,
        Probability probability) =>
        new(
            MarketType.CorrectScore,
            [
                new CorrectScoreSelection(
                    score,
                    probability,
                    ToSafeOdds(probability))
            ]);

    private static MarketWithBase CreateOverUnderMarket(
        MarketType type,
        MarketBase marketBase,
        Probability over)
    {
        var under = over.Inverse();

        return new MarketWithBase(
            type,
            marketBase,
            [
                CreateSelection(SelectionCode.Over, over),
                CreateSelection(SelectionCode.Under, under)
            ]);
    }

    private static MarketWithBase CreateHomeAwayMarket(
        MarketBase marketBase,
        Probability home)
    {
        var away = home.Inverse();

        return new MarketWithBase(
            MarketType.Handicap,
            marketBase,
            [
                CreateSelection(SelectionCode.Home, home),
                CreateSelection(SelectionCode.Away, away)
            ]);
    }

    private static Selection CreateSelection(
        SelectionCode code,
        Probability probability) =>
        new(
            code,
            probability,
            ToSafeOdds(probability));

    private static Odds ToSafeOdds(Probability probability)
    {
        var value = Math.Clamp(
            probability.Value,
            MinOddsProbability,
            1d - MinOddsProbability);

        return new Probability(value).ToOdds();
    }
}
