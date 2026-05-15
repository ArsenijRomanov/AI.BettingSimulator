namespace SportsBook.Infrastructure.Security;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; } = 60;
    public int RefreshTokenLifetimeDays { get; init; } = 30;
}
