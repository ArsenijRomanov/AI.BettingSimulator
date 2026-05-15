using Microsoft.EntityFrameworkCore;
using SportsBook.Api.Features.Common;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Enums;

namespace SportsBook.Api.Features.PlayerMatches;

public static class PlayerMatchEndpoints
{
    public static IEndpointRouteBuilder MapPlayerMatchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/player/matches")
            .WithTags("Player Matches");

        group.MapGet("", GetOpenMatches);
        group.MapGet("/{matchId:guid}", GetMatchById);

        return app;
    }

    private static async Task<IResult> GetOpenMatches(
        ISportsBookDbContext dbContext,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;

        var matches = await dbContext.Matches
            .AsNoTracking()
            .Where(match =>
                match.Status == MatchStatus.Open &&
                match.StartTime > now)
            .OrderBy(match => match.StartTime)
            .ToListAsync(cancellationToken);

        return Results.Ok(
            matches.Select(match => match.ToSummaryResponse()).ToList());
    }

    private static async Task<IResult> GetMatchById(
        Guid matchId,
        ISportsBookDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var match = await dbContext.Matches
            .AsNoTracking()
            .Include(match => match.Markets)
            .ThenInclude(market => market.Selections)
            .FirstOrDefaultAsync(match => match.Id == matchId, cancellationToken);

        if (match is null)
            return Results.NotFound();

        return Results.Ok(match.ToDetailsResponse());
    }
}
