using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Entities;

namespace SportsBook.Infrastructure.Security;

public sealed class JwtAuthTokenService : IAuthTokenService
{
    private readonly JwtOptions _options;

    public JwtAuthTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AccessTokenResult CreateAccessToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_options.Issuer))
            throw new InvalidOperationException("JWT issuer is missing.");

        if (string.IsNullOrWhiteSpace(_options.Audience))
            throw new InvalidOperationException("JWT audience is missing.");

        if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 32)
            throw new InvalidOperationException("JWT signing key must contain at least 32 characters.");

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("role", user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }

    public string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string HashRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be empty.", nameof(refreshToken));

        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }

    public DateTimeOffset GetRefreshTokenExpiresAt(DateTimeOffset now)
    {
        return now.AddDays(_options.RefreshTokenLifetimeDays);
    }
}
