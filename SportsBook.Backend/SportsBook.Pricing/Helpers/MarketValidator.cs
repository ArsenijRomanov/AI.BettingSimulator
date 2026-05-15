using SportsBook.Pricing.Abstractions;
using SportsBook.Pricing.Enums;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Helpers;

internal static class MarketValidator
{
    private const double SurebetTolerance = 1e-12;

    public static void Validate<TSelection>(
        MarketType type,
        IReadOnlyList<TSelection> selections)
        where TSelection : ISelection
    {
        ArgumentNullException.ThrowIfNull(selections);
        ArgumentOutOfRangeException.ThrowIfZero(selections.Count);

        if (type.RequiresBase())
            throw new ArgumentException($"{type} requires base.", nameof(type));

        switch (type)
        {
            case MarketType.HomeDrawAway:
                ValidateHomeDrawAway(selections);
                ValidateNoSurebet(selections);
                return;

            case MarketType.CorrectScore:
                ValidateCorrectScore(selections);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Market type is not supported.");
        }
    }

    public static void ValidateWithBase(
        MarketType type,
        IReadOnlyList<Selection> selections)
    {
        ArgumentNullException.ThrowIfNull(selections);
        ArgumentOutOfRangeException.ThrowIfZero(selections.Count);

        if (!type.RequiresBase())
            throw new ArgumentException($"{type} does not require base.", nameof(type));

        switch (type)
        {
            case MarketType.Total:
            case MarketType.HomeTotal:
            case MarketType.AwayTotal:
                ValidateOverUnder(selections);
                ValidateNoSurebet(selections);
                return;

            case MarketType.Handicap:
                ValidateHomeAway(selections);
                ValidateNoSurebet(selections);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Market type is not supported.");
        }
    }

    private static void ValidateHomeDrawAway<TSelection>(
        IReadOnlyList<TSelection> selections)
        where TSelection : ISelection
    {
        if (selections.Count != 3)
            throw new ArgumentException("HomeDrawAway market must contain exactly three selections.", nameof(selections));

        var hasHome = false;
        var hasDraw = false;
        var hasAway = false;

        foreach (var selection in selections)
        {
            switch (selection.Code)
            {
                case SelectionCode.Home when !hasHome:
                    hasHome = true;
                    break;

                case SelectionCode.Draw when !hasDraw:
                    hasDraw = true;
                    break;

                case SelectionCode.Away when !hasAway:
                    hasAway = true;
                    break;

                default:
                    throw new ArgumentException("HomeDrawAway market must contain Home, Draw and Away selections.", nameof(selections));
            }
        }
    }

    private static void ValidateOverUnder(IReadOnlyList<Selection> selections)
    {
        if (selections.Count != 2)
            throw new ArgumentException("Over/Under market must contain exactly two selections.", nameof(selections));

        var hasOver = false;
        var hasUnder = false;

        foreach (var selection in selections)
        {
            switch (selection.Code)
            {
                case SelectionCode.Over when !hasOver:
                    hasOver = true;
                    break;

                case SelectionCode.Under when !hasUnder:
                    hasUnder = true;
                    break;

                default:
                    throw new ArgumentException("Over/Under market must contain Over and Under selections.", nameof(selections));
            }
        }
    }

    private static void ValidateHomeAway(IReadOnlyList<Selection> selections)
    {
        if (selections.Count != 2)
            throw new ArgumentException("Handicap market must contain exactly two selections.", nameof(selections));

        var hasHome = false;
        var hasAway = false;

        foreach (var selection in selections)
        {
            switch (selection.Code)
            {
                case SelectionCode.Home when !hasHome:
                    hasHome = true;
                    break;

                case SelectionCode.Away when !hasAway:
                    hasAway = true;
                    break;

                default:
                    throw new ArgumentException("Handicap market must contain Home and Away selections.", nameof(selections));
            }
        }
    }

    private static void ValidateCorrectScore<TSelection>(
        IReadOnlyList<TSelection> selections)
        where TSelection : ISelection
    {
        foreach (var selection in selections)
        {
            if (selection is not CorrectScoreSelection)
                throw new ArgumentException("CorrectScore market must contain only CorrectScoreSelection items.", nameof(selections));

            if (selection.Code != SelectionCode.ExactScore)
                throw new ArgumentException("CorrectScore market must contain only ExactScore selections.", nameof(selections));
        }

        for (var i = 0; i < selections.Count; i++)
        {
            var first = (CorrectScoreSelection)(ISelection)selections[i];

            for (var j = i + 1; j < selections.Count; j++)
            {
                var second = (CorrectScoreSelection)(ISelection)selections[j];

                if (first.Score == second.Score)
                    throw new ArgumentException($"Correct score {first.Score} is duplicated.", nameof(selections));
            }
        }
    }

    private static void ValidateNoSurebet<TSelection>(
        IReadOnlyList<TSelection> selections)
        where TSelection : ISelection
    {
        var impliedProbabilitySum = 0d;

        foreach (var selection in selections)
            impliedProbabilitySum += selection.Odds.ToProbability().Value;

        if (impliedProbabilitySum < 1d - SurebetTolerance)
            throw new ArgumentException("Market odds form an arbitrage opportunity.", nameof(selections));
    }
}