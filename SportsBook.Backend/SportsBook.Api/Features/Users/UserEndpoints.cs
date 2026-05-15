using SportsBook.Api.Extensions;
using SportsBook.Application.UseCases.Users;

namespace SportsBook.Api.Features.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        users.MapGet("/me", GetCurrentUser);

        var playerProfile = app.MapGroup("/api/player/profile")
            .WithTags("Player Profile")
            .RequireAuthorization("PlayerOnly");

        playerProfile.MapPut("", UpdatePlayerProfile);

        return app;
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext httpContext,
        GetCurrentUserHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.GetRequiredUserId();

        var result = await handler.Handle(
            new GetCurrentUserQuery(userId),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdatePlayerProfile(
        UpdatePlayerProfileRequest request,
        HttpContext httpContext,
        UpdatePlayerProfileHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.GetRequiredUserId();

        var result = await handler.Handle(
            new UpdatePlayerProfileCommand(
                userId,
                request.DisplayName),
            cancellationToken);

        return Results.Ok(result);
    }

    private sealed record UpdatePlayerProfileRequest(
        string DisplayName);
}
