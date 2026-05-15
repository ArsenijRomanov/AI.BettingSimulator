using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsBook.Application.Abstractions;
using SportsBook.Infrastructure.Persistence;
using SportsBook.Infrastructure.Time;

namespace SportsBook.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'Postgres' is missing.");

        services.AddDbContext<SportsBookDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<ISportsBookDbContext>(provider =>
            provider.GetRequiredService<SportsBookDbContext>());

        services.AddScoped<IFinancialLockService, PostgresFinancialLockService>();

        services.AddSingleton<IClock, SystemClock>();

        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
