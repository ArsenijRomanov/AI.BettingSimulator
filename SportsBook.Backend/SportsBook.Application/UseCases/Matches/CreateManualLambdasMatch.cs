using SportsBook.Application.Abstractions;
using SportsBook.Application.Common;
using SportsBook.Application.Pricing;
using SportsBook.Domain.Entities;
using SportsBook.Domain.Enums;

namespace SportsBook.Application.UseCases.Matches;

public sealed record CreateManualLambdasMatchCommand(
    string HomeTeamName,
    string AwayTeamName,
    string Competition,
    DateTimeOffset StartTime,
    string? Venue,
    double LambdaHome,
    double LambdaAway);

public sealed record CreateManualLambdasMatchResult(
    Guid MatchId,
    MatchStatus Status,
    PricingMode PricingMode,
    string HomeTeamName,
    string AwayTeamName,
    string Competition,
    DateTimeOffset StartTime,
    string? Venue,
    double LambdaHome,
    double LambdaAway,
    IReadOnlyList<MarketPreviewDto> FairMarketsPreview);

public sealed class CreateManualLambdasMatchHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly PricingPreviewFactory _pricingPreviewFactory;

    public CreateManualLambdasMatchHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        PricingPreviewFactory pricingPreviewFactory)
    {
        _dbContext = dbContext;
        _clock = clock;
        _pricingPreviewFactory = pricingPreviewFactory;
    }

    public async Task<CreateManualLambdasMatchResult> Handle(
        CreateManualLambdasMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        if (command.StartTime <= now)
            throw new InvalidOperationException("Match start time must be in the future.");

        var match = new Match(
            id: Guid.NewGuid(),
            homeTeamName: command.HomeTeamName,
            awayTeamName: command.AwayTeamName,
            competition: command.Competition,
            startTime: command.StartTime,
            venue: command.Venue,
            lambdaHome: command.LambdaHome,
            lambdaAway: command.LambdaAway,
            pricingMode: PricingMode.ManualLambdas,
            modelVersion: null,
            createdAt: now);

        _dbContext.Matches.Add(match);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var preview = _pricingPreviewFactory.CreateDefaultPreview(
            match.LambdaHome,
            match.LambdaAway);

        return new CreateManualLambdasMatchResult(
            match.Id,
            match.Status,
            match.PricingMode,
            match.HomeTeamName,
            match.AwayTeamName,
            match.Competition,
            match.StartTime,
            match.Venue,
            match.LambdaHome,
            match.LambdaAway,
            preview);
    }
}
