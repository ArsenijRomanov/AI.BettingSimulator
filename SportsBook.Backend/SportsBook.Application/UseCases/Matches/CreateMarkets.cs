using SportsBook.Application.Abstractions;
using SportsBook.Application.Common;
using SportsBook.Application.Pricing;
using SportsBook.Domain.Entities;
using SportsBook.Domain.Enums;

namespace SportsBook.Application.UseCases.Matches;

public sealed record CreateMarketsCommand(
    Guid MatchId,
    IReadOnlyList<MarketRequestDto> Markets);

public sealed record CreateMarketsResult(
    Guid MatchId,
    MatchStatus Status,
    int MarketsCreated,
    IReadOnlyList<MarketDto> Markets);

public sealed class CreateMarketsHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly PricingPreviewFactory _pricingPreviewFactory;
    private readonly IFinancialLockService _financialLockService;

    public CreateMarketsHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        PricingPreviewFactory pricingPreviewFactory,
        IFinancialLockService financialLockService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _pricingPreviewFactory = pricingPreviewFactory;
        _financialLockService = financialLockService;
    }

    public async Task<CreateMarketsResult> Handle(
        CreateMarketsCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Markets.Count == 0)
            throw new InvalidOperationException("At least one market must be selected.");

        var now = _clock.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var match = await _financialLockService.LockMatchWithMarketsForUpdateAsync(
            command.MatchId,
            cancellationToken);

        if (match is null)
            throw new InvalidOperationException("Match was not found.");

        foreach (var marketRequest in command.Markets)
        {
            var preview = _pricingPreviewFactory.CreatePreview(
                match.LambdaHome,
                match.LambdaAway,
                marketRequest.Type,
                marketRequest.Base,
                marketRequest.ExactScores);

            var market = new Market(
                id: Guid.NewGuid(),
                matchId: match.Id,
                type: marketRequest.Type,
                @base: marketRequest.Base,
                margin: marketRequest.Margin);

            foreach (var previewSelection in preview.Selections)
            {
                var finalOdds = previewSelection.FairProbability.ToOdds(marketRequest.Margin);

                var selection = new Selection(
                    id: Guid.NewGuid(),
                    marketId: market.Id,
                    code: previewSelection.Code,
                    name: PricingPreviewFactory.CreateSelectionName(
                        marketRequest.Type,
                        marketRequest.Base,
                        previewSelection.Code,
                        previewSelection.ExactScore),
                    fairProbability: previewSelection.FairProbability,
                    fairOdds: previewSelection.FairOdds,
                    odds: finalOdds,
                    oddsVersion: 1,
                    exactScore: previewSelection.ExactScore);

                market.AddSelection(selection);
            }

            match.AddMarket(market, now);
        }

        match.Open(now);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CreateMarketsResult(
            match.Id,
            match.Status,
            command.Markets.Count,
            match.Markets.Select(ToDto).ToList());
    }

    private static MarketDto ToDto(Market market)
    {
        return new MarketDto(
            market.Id,
            market.Type,
            market.Base,
            market.Margin,
            market.Selections.Select(selection => new SelectionDto(
                selection.Id,
                selection.Code,
                selection.Name,
                selection.FairProbability,
                selection.FairOdds,
                selection.Odds,
                selection.OddsVersion,
                selection.ExactScore)).ToList());
    }
}
