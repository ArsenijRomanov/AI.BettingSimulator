using SportsBook.Application.UseCases.Auth;

namespace SportsBook.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register-player", RegisterPlayer).AllowAnonymous();
        group.MapPost("/register-operator", RegisterOperator).AllowAnonymous();
        group.MapPost("/login", Login).AllowAnonymous();
        group.MapPost("/refresh", Refresh).AllowAnonymous();
        group.MapPost("/logout", Logout).AllowAnonymous();

        return app;
    }

    private static async Task<IResult> RegisterPlayer(
        RegisterPlayerRequest request,
        RegisterPlayerHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new RegisterPlayerCommand(
                request.Email,
                request.Password,
                request.DisplayName,
                request.InitialBalance),
            cancellationToken);

        return Results.Created("/api/users/me", result);
    }

    private static async Task<IResult> RegisterOperator(
        RegisterOperatorRequest request,
        RegisterOperatorHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new RegisterOperatorCommand(
                request.Email,
                request.Password),
            cancellationToken);

        return Results.Created("/api/users/me", result);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        LoginHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new LoginCommand(
                request.Email,
                request.Password),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> Refresh(
        RefreshTokenRequest request,
        RefreshTokenHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> Logout(
        LogoutRequest request,
        LogoutHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new LogoutCommand(request.RefreshToken),
            cancellationToken);

        return Results.Ok(result);
    }

    private sealed record RegisterPlayerRequest(
        string Email,
        string Password,
        string DisplayName,
        decimal InitialBalance = 0m);

    private sealed record RegisterOperatorRequest(
        string Email,
        string Password);

    private sealed record LoginRequest(
        string Email,
        string Password);

    private sealed record RefreshTokenRequest(
        string RefreshToken);

    private sealed record LogoutRequest(
        string RefreshToken);
}
