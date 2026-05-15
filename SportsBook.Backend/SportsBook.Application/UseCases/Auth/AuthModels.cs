using SportsBook.Domain.Enums;

namespace SportsBook.Application.UseCases.Auth;

public sealed record AuthResult(
    Guid UserId,
    string Email,
    UserRole Role,
    string? DisplayName,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
    