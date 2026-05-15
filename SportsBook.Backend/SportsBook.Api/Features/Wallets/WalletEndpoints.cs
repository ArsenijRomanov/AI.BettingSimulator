using SportsBook.Api.Extensions;
using SportsBook.Application.UseCases.Wallets;

namespace SportsBook.Api.Features.Wallets;

public static class WalletEndpoints
{
    public static IEndpointRouteBuilder MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wallet")
            .WithTags("Wallet");

        group.MapPost("/create", CreateWallet);
        group.MapGet("", GetWallet);
        group.MapPost("/deposit", DepositWallet);

        return app;
    }

    private static async Task<IResult> CreateWallet(
        CreateWalletRequest request,
        HttpContext httpContext,
        CreateWalletHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.GetRequiredUserId();

        var result = await handler.Handle(
            new CreateWalletCommand(
                userId,
                request.InitialBalance),
            cancellationToken);

        return Results.Created("/api/wallet", result);
    }

    private static async Task<IResult> GetWallet(
        HttpContext httpContext,
        GetWalletHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.GetRequiredUserId();

        var result = await handler.Handle(
            new GetWalletQuery(userId),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> DepositWallet(
        DepositWalletRequest request,
        HttpContext httpContext,
        DepositWalletHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.GetRequiredUserId();

        var result = await handler.Handle(
            new DepositWalletCommand(
                userId,
                request.Amount),
            cancellationToken);

        return Results.Ok(result);
    }

    private sealed record CreateWalletRequest(
        decimal InitialBalance = 0m);

    private sealed record DepositWalletRequest(
        decimal Amount);
}
