using SportsBook.Application.Abstractions;
using SportsBook.Application.Common;
using SportsBook.Application.Pricing;
using SportsBook.Domain.Entities;
using SportsBook.Domain.Enums;

namespace SportsBook.Application.UseCases.Matches;

public sealed record CreateModelMatchCommand(
    string HomeTeamName,
    string AwayTeamName,
    string Competition,
    DateTimeOffset StartTime,
    string? Venue);

public sealed record CreateModelMatchResult(
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
    string? ModelVersion,
    bool IsStubPrediction,
    IReadOnlyList<MarketPreviewDto> FairMarketsPreview);

public sealed class CreateModelMatchHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IMatchPredictionClient _matchPredictionClient;
    private readonly PricingPreviewFactory _pricingPreviewFactory;

    public CreateModelMatchHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        IMatchPredictionClient matchPredictionClient,
        PricingPreviewFactory pricingPreviewFactory)
    {
        _dbContext = dbContext;
        _clock = clock;
        _matchPredictionClient = matchPredictionClient;
        _pricingPreviewFactory = pricingPreviewFactory;
    }

    public async Task<CreateModelMatchResult> Handle(
        CreateModelMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        if (command.StartTime <= now)
            throw new InvalidOperationException("Match start time must be in the future.");

        var prediction = await _matchPredictionClient.PredictLambdasAsync(
            new MatchLambdaPredictionRequest(
                command.HomeTeamName,
                command.AwayTeamName,
                command.Competition,
                command.StartTime),
            cancellationToken);

        var match = new Match(
            id: Guid.NewGuid(),
            homeTeamName: command.HomeTeamName,
            awayTeamName: command.AwayTeamName,
            competition: command.Competition,
            startTime: command.StartTime,
            venue: command.Venue,
            lambdaHome: prediction.LambdaHome,
            lambdaAway: prediction.LambdaAway,
            pricingMode: PricingMode.Model,
            modelVersion: prediction.ModelVersion,
            createdAt: now);

        _dbContext.Matches.Add(match);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var preview = _pricingPreviewFactory.CreateDefaultPreview(
            match.LambdaHome,
            match.LambdaAway);

        return new CreateModelMatchResult(
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
            match.ModelVersion,
            prediction.IsStub,
            preview);
    }
}
