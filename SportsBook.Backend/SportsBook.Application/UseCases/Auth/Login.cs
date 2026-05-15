using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Entities;
using SportsBook.Domain.Enums;

namespace SportsBook.Application.UseCases.Auth;

public sealed record LoginCommand(
    string Email,
    string Password);

public sealed class LoginHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenService _authTokenService;

    public LoginHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        IPasswordHasher passwordHasher,
        IAuthTokenService authTokenService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _passwordHasher = passwordHasher;
        _authTokenService = authTokenService;
    }

    public async Task<AuthResult> Handle(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var normalizedEmail = EmailNormalizer.Normalize(command.Email);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid email or password.");

        user.EnsureActive();
        user.MarkLoggedIn(now);

        var displayName = user.Role == UserRole.Player
            ? await _dbContext.PlayerProfiles
                .Where(profile => profile.UserId == user.Id)
                .Select(profile => profile.DisplayName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var accessToken = _authTokenService.CreateAccessToken(user);
        var refreshToken = _authTokenService.CreateRefreshToken();
        var refreshTokenHash = _authTokenService.HashRefreshToken(refreshToken);
        var refreshTokenExpiresAt = _authTokenService.GetRefreshTokenExpiresAt(now);

        _dbContext.RefreshTokens.Add(new RefreshToken(
            id: Guid.NewGuid(),
            userId: user.Id,
            tokenHash: refreshTokenHash,
            createdAt: now,
            expiresAt: refreshTokenExpiresAt));

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AuthResult(
            user.Id,
            user.Email,
            user.Role,
            displayName,
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken,
            refreshTokenExpiresAt);
    }
}
