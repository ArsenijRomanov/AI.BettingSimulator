using Microsoft.Extensions.DependencyInjection;
using SportsBook.Application.Pricing;
using SportsBook.Application.UseCases.Bets;
using SportsBook.Application.UseCases.Matches;
using SportsBook.Application.UseCases.Wallets;
using SportsBook.Domain.Services;

namespace SportsBook.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<PricingPreviewFactory>();
        services.AddSingleton<SelectionSettlementService>();

        services.AddScoped<CreateManualLambdasMatchHandler>();
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