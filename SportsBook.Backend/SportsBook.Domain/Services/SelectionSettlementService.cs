using SportsBook.Domain.Entities;
using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Domain.Services;

public sealed class SelectionSettlementService
{
    public bool IsWinningSelection(
        Market market,
        Selection selection,
        Score finalScore)
    {
        if (selection.MarketId != market.Id)
            throw new InvalidOperationException("Selection belongs to another market.");

        return market.Type switch
        {
            MarketType.HomeDrawAway => IsWinningHomeDrawAway(selection, finalScore),
            MarketType.Total => IsWinningTotal(market, selection, finalScore),
            MarketType.HomeTotal => IsWinningHomeTotal(market, selection, finalScore),
            MarketType.AwayTotal => IsWinningAwayTotal(market, selection, finalScore),
            MarketType.Handicap => IsWinningHandicap(market, selection, finalScore),
            MarketType.CorrectScore => IsWinningCorrectScore(selection, finalScore),

            _ => throw new NotSupportedException($"Market type {market.Type} is not supported.")
        };
    }

    private static bool IsWinningHomeDrawAway(
        Selection selection,
        Score score)
    {
        return selection.Code switch
        {
            SelectionCode.Home => score.Home > score.Away,
            SelectionCode.Draw => score.Home == score.Away,
            SelectionCode.Away => score.Home < score.Away,

            _ => throw new InvalidOperationException($"Selection {selection.Code} is invalid for HomeDrawAway market.")
        };
    }

    private static bool IsWinningTotal(
        Market market,
        Selection selection,
        Score score)
    {
        var line = GetRequiredBaseValue(market);

        return selection.Code switch
        {
            SelectionCode.Over => score.Total > line,
            SelectionCode.Under => score.Total < line,

            _ => throw new InvalidOperationException($"Selection {selection.Code} is invalid for Total market.")
        };
    }

    private static bool IsWinningHomeTotal(
        Market market,
        Selection selection,
        Score score)
    {
        var line = GetRequiredBaseValue(market);

        return selection.Code switch
        {
            SelectionCode.Over => score.Home > line,
            SelectionCode.Under => score.Home < line,

            _ => throw new InvalidOperationException($"Selection {selection.Code} is invalid for HomeTotal market.")
        };
    }

    private static bool IsWinningAwayTotal(
        Market market,
        Selection selection,
        Score score)
    {
        var line = GetRequiredBaseValue(market);

        return selection.Code switch
        {
            SelectionCode.Over => score.Away > line,
            SelectionCode.Under => score.Away < line,

            _ => throw new InvalidOperationException($"Selection {selection.Code} is invalid for AwayTotal market.")
        };
    }

    private static bool IsWinningHandicap(
        Market market,
        Selection selection,
        Score score)
    {
        var homeHandicap = GetRequiredBaseValue(market);
        var adjustedHomeScore = score.Home + homeHandicap;

        return selection.Code switch
        {
            SelectionCode.Home => adjustedHomeScore > score.Away,
            SelectionCode.Away => adjustedHomeScore < score.Away,

            _ => throw new InvalidOperationException($"Selection {selection.Code} is invalid for Handicap market.")
        };
    }

    private static bool IsWinningCorrectScore(
        Selection selection,
        Score score)
    {
        if (selection.Code != SelectionCode.ExactScore)
            throw new InvalidOperationException($"Selection {selection.Code} is invalid for CorrectScore market.");

        return selection.ExactScore == score;
    }

    private static double GetRequiredBaseValue(Market market)
    {
        if (market.Base is null)
            throw new InvalidOperationException($"Market {market.Type} requires base.");

        return market.Base.Value.Value;
    }
}
