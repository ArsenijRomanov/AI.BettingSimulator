using Microsoft.Extensions.DependencyInjection;
using SportsBook.Application.Pricing;
using SportsBook.Application.UseCases.Auth;
using SportsBook.Application.UseCases.Bets;
using SportsBook.Application.UseCases.Matches;
using SportsBook.Application.UseCases.Users;
using SportsBook.Application.UseCases.Wallets;
using SportsBook.Domain.Services;

namespace SportsBook.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<PricingPreviewFactory>();
        services.AddSingleton<SelectionSettlementService>();

        services.AddScoped<RegisterPlayerHandler>();
        services.AddScoped<RegisterOperatorHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<LogoutHandler>();

        services.AddScoped<GetCurrentUserHandler>();
        services.AddScoped<UpdatePlayerProfileHandler>();

        services.AddScoped<CreateManualLambdasMatchHandler>();
        services.AddScoped<CreateModelMatchHandler>();
        services.AddScoped<CreateMarketsHandler>();
        services.AddScoped<PlaceBetHandler>();
        services.AddScoped<SettleMatchHandler>();
        services.AddScoped<CancelMatchHandler>();

        services.AddScoped<CreateWalletHandler>();
        services.AddScoped<GetWalletHandler>();
        services.AddScoped<DepositWalletHandler>();

        return services;
    }
}
