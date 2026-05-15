using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportsBook.Application.Abstractions;
using SportsBook.Infrastructure.ML;
using SportsBook.Infrastructure.Persistence;
using SportsBook.Infrastructure.Security;
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

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<MlServiceOptions>(configuration.GetSection("MlService"));

        services.AddDbContext<SportsBookDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<ISportsBookDbContext>(provider =>
            provider.GetRequiredService<SportsBookDbContext>());

        services.AddScoped<IFinancialLockService, PostgresFinancialLockService>();
        services.AddScoped<IAuthLockService, PostgresAuthLockService>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAuthTokenService, JwtAuthTokenService>();

        services.AddHttpClient<IMatchPredictionClient, HttpMatchPredictionClient>((provider, client) =>
        {
            var options = configuration
                .GetSection("MlService")
                .Get<MlServiceOptions>();

            if (options is null || string.IsNullOrWhiteSpace(options.BaseUrl))
                throw new InvalidOperationException("ML service base URL is missing.");

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}
