using SportsBook.Pricing.Enums;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Markets;

internal static class MarketFactory
{
    private const double MinOddsProbability = 1e-12;

    public static Market<Selection> CreateHomeDrawAway(
        Probability home,
        Probability draw,
        Probability away,
        double margin = 0d)
    {
        if (!Probability.SumApproximatelyEqualsOne(home, draw, away))
            throw new ArgumentException("HomeDrawAway probabilities must sum to 1.");

        return new Market<Selection>(
            MarketType.HomeDrawAway,
            [
                CreateSelection(SelectionCode.Home, home, margin),
                CreateSelection(SelectionCode.Draw, draw, margin),
                CreateSelection(SelectionCode.Away, away, margin)
            ]);
    }

    public static MarketWithBase CreateTotalFromOver(
        MarketBase marketBase,
        Probability over,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.Total,
            marketBase,
            over,
            margin);

    public static MarketWithBase CreateTotalFromUnder(
        MarketBase marketBase,
        Probability under,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.Total,
            marketBase,
            under.Inverse(),
            margin);

    public static MarketWithBase CreateHomeTotalFromOver(
        MarketBase marketBase,
        Probability over,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.HomeTotal,
            marketBase,
            over,
            margin);

    public static MarketWithBase CreateHomeTotalFromUnder(
        MarketBase marketBase,
        Probability under,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.HomeTotal,
            marketBase,
            under.Inverse(),
            margin);

    public static MarketWithBase CreateAwayTotalFromOver(
        MarketBase marketBase,
        Probability over,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.AwayTotal,
            marketBase,
            over,
            margin);

    public static MarketWithBase CreateAwayTotalFromUnder(
        MarketBase marketBase,
        Probability under,
        double margin = 0d) =>
        CreateOverUnderMarket(
            MarketType.AwayTotal,
            marketBase,
            under.Inverse(),
            margin);

    public static MarketWithBase CreateHandicapFromHome(
        MarketBase marketBase,
        Probability home,
        double margin = 0d) =>
        CreateHomeAwayMarket(
            marketBase,
            home,
            margin);

    public static MarketWithBase CreateHandicapFromAway(
        MarketBase marketBase,
        Probability away,
        double margin = 0d) =>
        CreateHomeAwayMarket(
            marketBase,
            away.Inverse(),
            margin);

    public static Market<CorrectScoreSelection> CreateCorrectScore(
        Score score,
        Probability probability,
        double margin = 0d) =>
        new(
            MarketType.CorrectScore,
            [
                new CorrectScoreSelection(
                    score,
                    probability,
                    ToSafeOdds(probability, margin))
            ]);

    private static MarketWithBase CreateOverUnderMarket(
        MarketType type,
        MarketBase marketBase,
        Probability over,
        double margin)
    {
        var under = over.Inverse();

        return new MarketWithBase(
            type,
            marketBase,
            [
                CreateSelection(SelectionCode.Over, over, margin),
                CreateSelection(SelectionCode.Under, under, margin)
            ]);
    }

    private static MarketWithBase CreateHomeAwayMarket(
        MarketBase marketBase,
        Probability home,
        double margin)
    {
        var away = home.Inverse();

        return new MarketWithBase(
            MarketType.Handicap,
            marketBase,
            [
                CreateSelection(SelectionCode.Home, home, margin),
                CreateSelection(SelectionCode.Away, away, margin)
            ]);
    }

    private static Selection CreateSelection(
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

        bookmakerProbability = Math.Clamp(
            bookmakerProbability,
            MinOddsProbability,
            1d - MinOddsProbability);

        return new Odds(1d / bookmakerProbability);
    }
}