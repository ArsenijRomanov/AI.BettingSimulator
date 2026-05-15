using SportsBook.Application.Common;
using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;
using SportsBook.Pricing.Maths;
using SportsBook.Pricing.Markets;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Application.Pricing;

public sealed class PricingPreviewFactory
{
    public IReadOnlyList<MarketPreviewDto> CreateDefaultPreview(
        double lambdaHome,
        double lambdaAway)
    {
        var generator = new MarketGenerator(lambdaHome, lambdaAway);

        var result = new List<MarketPreviewDto>
        {
            ToRegularPreview(generator.GenerateHomeDrawAway()),
            ToBasePreview(generator.GenerateTotal(new MarketBase(2.5))),
            ToBasePreview(generator.GenerateHandicap(new MarketBase(-1.5)))
        };

        return result;
    }

    public MarketPreviewDto CreatePreview(
        double lambdaHome,
        double lambdaAway,
        MarketType type,
        MarketBase? marketBase,
        IReadOnlyList<Score>? exactScores = null)
    {
        var generator = new MarketGenerator(lambdaHome, lambdaAway);

        return type switch
        {
            MarketType.HomeDrawAway => ToRegularPreview(
                generator.GenerateHomeDrawAway()),

            MarketType.Total => ToBasePreview(
                generator.GenerateTotal(RequireBase(type, marketBase))),

            MarketType.HomeTotal => ToBasePreview(
                generator.GenerateHomeTotal(RequireBase(type, marketBase))),

            MarketType.AwayTotal => ToBasePreview(
                generator.GenerateAwayTotal(RequireBase(type, marketBase))),

            MarketType.Handicap => ToBasePreview(
                generator.GenerateHandicap(RequireBase(type, marketBase))),

            MarketType.CorrectScore => CreateCorrectScorePreview(
                generator,
                exactScores),

            _ => throw new NotSupportedException($"Market type {type} is not supported for preview.")
        };
    }

    private static MarketPreviewDto ToRegularPreview(
        PricedMarket<PricedSelection> market)
    {
        return new MarketPreviewDto(
            market.Type,
            Base: null,
            market.Selections
                .Select(selection => new SelectionPreviewDto(
                    selection.Code,
                    CreateSelectionName(market.Type, null, selection.Code, null),
                    selection.Probability,
                    selection.Odds,
                    ExactScore: null))
                .ToList());
    }

    private static MarketPreviewDto ToBasePreview(
        PricedMarketWithBase market)
    {
        return new MarketPreviewDto(
            market.Type,
            market.Base,
            market.Selections
                .Select(selection => new SelectionPreviewDto(
                    selection.Code,
                    CreateSelectionName(market.Type, market.Base, selection.Code, null),
                    selection.Probability,
                    selection.Odds,
                    ExactScore: null))
                .ToList());
    }

    private static MarketPreviewDto CreateCorrectScorePreview(
        MarketGenerator generator,
        IReadOnlyList<Score>? exactScores)
    {
        if (exactScores is null || exactScores.Count == 0)
            throw new InvalidOperationException("CorrectScore market requires at least one exact score.");

        var selections = new List<SelectionPreviewDto>();
        var uniqueScores = new HashSet<Score>();

        foreach (var score in exactScores)
        {
            if (!uniqueScores.Add(score))
                throw new InvalidOperationException($"CorrectScore market contains duplicated score {score}.");

            var pricedMarket = generator.GenerateCorrectScore(score);
            var pricedSelection = pricedMarket.Selections.Single();

            selections.Add(new SelectionPreviewDto(
                pricedSelection.Code,
                CreateSelectionName(
                    MarketType.CorrectScore,
                    marketBase: null,
                    pricedSelection.Code,
                    pricedSelection.Score),
                pricedSelection.Probability,
                pricedSelection.Odds,
                pricedSelection.Score));
        }

        return new MarketPreviewDto(
            MarketType.CorrectScore,
            Base: null,
            selections);
    }

    private static MarketBase RequireBase(
        MarketType type,
        MarketBase? marketBase)
    {
        return marketBase ?? throw new InvalidOperationException($"Market type {type} requires base.");
    }

    public static string CreateSelectionName(
        MarketType type,
        MarketBase? marketBase,
        SelectionCode code,
        Score? exactScore)
    {
        return type switch
        {
            MarketType.HomeDrawAway => code switch
            {
                SelectionCode.Home => "Home Win",
                SelectionCode.Draw => "Draw",
                SelectionCode.Away => "Away Win",
                _ => throw new InvalidOperationException($"Invalid selection {code} for {type}.")
            },

            MarketType.Total => code switch
            {
                SelectionCode.Over => $"Over {marketBase}",
                SelectionCode.Under => $"Under {marketBase}",
                _ => throw new InvalidOperationException($"Invalid selection {code} for {type}.")
            },

            MarketType.HomeTotal => code switch
            {
                SelectionCode.Over => $"Home Over {marketBase}",
                SelectionCode.Under => $"Home Under {marketBase}",
                _ => throw new InvalidOperationException($"Invalid selection {code} for {type}.")
            },

            MarketType.AwayTotal => code switch
            {
                SelectionCode.Over => $"Away Over {marketBase}",
                SelectionCode.Under => $"Away Under {marketBase}",
                _ => throw new InvalidOperationException($"Invalid selection {code} for {type}.")
            },

            MarketType.Handicap => code switch
            {
                SelectionCode.Home => $"Home {marketBase}",
                SelectionCode.Away => $"Away {marketBase?.Opposite()}",
                _ => throw new InvalidOperationException($"Invalid selection {code} for {type}.")
            },

            MarketType.CorrectScore => exactScore is null
                ? throw new InvalidOperationException("CorrectScore selection requires exact score.")
                : $"Correct Score {exactScore}",

            _ => throw new NotSupportedException($"Market type {type} is not supported.")
        };
    }
}
