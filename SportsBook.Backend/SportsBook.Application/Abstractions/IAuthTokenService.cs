using SportsBook.Domain.Entities;

namespace SportsBook.Application.Abstractions;

public sealed record AccessTokenResult(
    string Token,
    DateTimeOffset ExpiresAt);

public interface IAuthTokenService
{
    AccessTokenResult CreateAccessToken(User user);

    string CreateRefreshToken();

    string HashRefreshToken(string refreshToken);

    DateTimeOffset GetRefreshTokenExpiresAt(DateTimeOffset now);
}
