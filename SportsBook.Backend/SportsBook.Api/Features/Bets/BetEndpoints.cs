using SportsBook.Api.Extensions;
using SportsBook.Application.UseCases.Bets;

namespace SportsBook.Api.Features.Bets;

public static class BetEndpoints
{
    public static IEndpointRouteBuilder MapBetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bets")
            .WithTags("Bets");

        group.MapPost("", PlaceBet);

        return app;
    }

    private static async Task<IResult> PlaceBet(
        PlaceBetRequest request,
        HttpContext httpContext,
        PlaceBetHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.GetRequiredUserId();

        var result = await handler.Handle(
            new PlaceBetCommand(
                userId,
                request.MatchId,
                request.MarketId,
                request.SelectionId,
                request.Stake,
                request.ExpectedOdds,
                request.OddsVersion),
            cancellationToken);

        return Results.Created($"/api/bets/{result.BetId}", result);
    }

    private sealed record PlaceBetRequest(
        Guid MatchId,
        Guid MarketId,
        Guid SelectionId,
        decimal Stake,
        double ExpectedOdds,
        int OddsVersion);
}
