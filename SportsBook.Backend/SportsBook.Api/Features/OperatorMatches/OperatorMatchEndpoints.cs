using SportsBook.Api.Features.Common;
using SportsBook.Application.Common;
using SportsBook.Application.UseCases.Matches;
using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Api.Features.OperatorMatches;

public static class OperatorMatchEndpoints
{
    public static IEndpointRouteBuilder MapOperatorMatchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/operator/matches")
            .WithTags("Operator Matches")
            .RequireAuthorization("OperatorOnly");

        group.MapPost("/manual-lambdas", CreateManualLambdasMatch);
        group.MapPost("/model", CreateModelMatch);
        group.MapPost("/{matchId:guid}/markets", CreateMarkets);
        group.MapPost("/{matchId:guid}/settle", SettleMatch);
        group.MapPost("/{matchId:guid}/cancel", CancelMatch);

        return app;
    }

    private static async Task<IResult> CreateManualLambdasMatch(
        CreateManualLambdasMatchRequest request,
        CreateManualLambdasMatchHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new CreateManualLambdasMatchCommand(
                request.HomeTeamName,
                request.AwayTeamName,
                request.Competition,
                request.StartTime,
                request.Venue,
                request.LambdaHome,
                request.LambdaAway),
            cancellationToken);

        var response = new CreateManualLambdasMatchResponse(
            result.MatchId,
            result.Status.ToString(),
            result.PricingMode.ToString(),
            result.HomeTeamName,
            result.AwayTeamName,
            result.Competition,
            result.StartTime,
            result.Venue,
            result.LambdaHome,
            result.LambdaAway,
            result.FairMarketsPreview
                .Select(market => market.ToApiResponse())
                .ToList());

        return Results.Created($"/api/player/matches/{result.MatchId}", response);
    }

    private static async Task<IResult> CreateModelMatch(
        CreateModelMatchRequest request,
        CreateModelMatchHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new CreateModelMatchCommand(
                request.HomeTeamName,
                request.AwayTeamName,
                request.Competition,
                request.StartTime,
                request.Venue),
            cancellationToken);

        var response = new CreateModelMatchResponse(
            result.MatchId,
            result.Status.ToString(),
            result.PricingMode.ToString(),
            result.HomeTeamName,
            result.AwayTeamName,
            result.Competition,
            result.StartTime,
            result.Venue,
            result.LambdaHome,
            result.LambdaAway,
            result.ModelVersion,
            result.IsStubPrediction,
            result.FairMarketsPreview
                .Select(market => market.ToApiResponse())
                .ToList());

        return Results.Created($"/api/player/matches/{result.MatchId}", response);
    }

    private static async Task<IResult> CreateMarkets(
        Guid matchId,
        CreateMarketsRequest request,
        CreateMarketsHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateMarketsCommand(
            matchId,
            request.Markets.Select(ToMarketRequestDto).ToList());

        var result = await handler.Handle(command, cancellationToken);

        var response = new CreateMarketsResponse(
            result.MatchId,
            result.Status.ToString(),
            result.MarketsCreated,
            result.Markets.Select(market => market.ToApiResponse()).ToList());

        return Results.Ok(response);
    }

    private static async Task<IResult> SettleMatch(
        Guid matchId,
        SettleMatchRequest request,
        SettleMatchHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new SettleMatchCommand(
                matchId,
                request.HomeScore,
                request.AwayScore),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> CancelMatch(
        Guid matchId,
        CancelMatchRequest request,
        CancelMatchHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new CancelMatchCommand(
                matchId,
                request.Reason),
            cancellationToken);

        return Results.Ok(result);
    }

    private static MarketRequestDto ToMarketRequestDto(
        CreateMarketRequest request)
    {
        return new MarketRequestDto(
            request.Type,
            request.Base.HasValue
                ? new MarketBase(request.Base.Value)
                : null,
            request.Margin,
            request.ExactScores?
                .Select(score => new Score(score.Home, score.Away))
                .ToList());
    }

    private sealed record CreateManualLambdasMatchRequest(
        string HomeTeamName,
        string AwayTeamName,
        string Competition,
        DateTimeOffset StartTime,
        string? Venue,
        double LambdaHome,
        double LambdaAway);

    private sealed record CreateModelMatchRequest(
        string HomeTeamName,
        string AwayTeamName,
        string Competition,
        DateTimeOffset StartTime,
        string? Venue);

    private sealed record CreateManualLambdasMatchResponse(
        Guid MatchId,
        string Status,
        string PricingMode,
        string HomeTeamName,
        string AwayTeamName,
        string Competition,
        DateTimeOffset StartTime,
        string? Venue,
        double LambdaHome,
        double LambdaAway,
        IReadOnlyList<ApiMarketPreviewResponse> FairMarketsPreview);

    private sealed record CreateModelMatchResponse(
        Guid MatchId,
        string Status,
        string PricingMode,
        string HomeTeamName,
        string AwayTeamName,
        string Competition,
        DateTimeOffset StartTime,
        string? Venue,
        double LambdaHome,
        double LambdaAway,
        string? ModelVersion,
        bool IsStubPrediction,
        IReadOnlyList<ApiMarketPreviewResponse> FairMarketsPreview);

    private sealed record CreateMarketsRequest(
        IReadOnlyList<CreateMarketRequest> Markets);

    private sealed record CreateMarketRequest(
        MarketType Type,
        double? Base,
        double Margin,
        IReadOnlyList<CreateScoreRequest>? ExactScores = null);

    private sealed record CreateScoreRequest(
        int Home,
        int Away);

    private sealed record CreateMarketsResponse(
        Guid MatchId,
        string Status,
        int MarketsCreated,
        IReadOnlyList<ApiMarketResponse> Markets);

    private sealed record SettleMatchRequest(
        int HomeScore,
        int AwayScore);

    private sealed record CancelMatchRequest(
        string? Reason);
}
