using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Entities;
using SportsBook.Domain.Enums;

namespace SportsBook.Application.UseCases.Auth;

public sealed record RefreshTokenCommand(
    string RefreshToken);

public sealed class RefreshTokenHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IAuthTokenService _authTokenService;
    private readonly IAuthLockService _authLockService;

    public RefreshTokenHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        IAuthTokenService authTokenService,
        IAuthLockService authLockService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _authTokenService = authTokenService;
        _authLockService = authLockService;
    }

    public async Task<AuthResult> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
            throw new ArgumentException("Refresh token cannot be empty.", nameof(command.RefreshToken));

        var now = _clock.UtcNow;
        var oldTokenHash = _authTokenService.HashRefreshToken(command.RefreshToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var oldRefreshToken = await _authLockService.LockRefreshTokenByHashForUpdateAsync(
            oldTokenHash,
            cancellationToken);

        if (oldRefreshToken is null || !oldRefreshToken.IsActive(now))
            throw new InvalidOperationException("Refresh token is invalid.");

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == oldRefreshToken.UserId, cancellationToken);

        if (user is null)
            throw new InvalidOperationException("User was not found.");

        user.EnsureActive();

        var displayName = user.Role == UserRole.Player
            ? await _dbContext.PlayerProfiles
                .Where(profile => profile.UserId == user.Id)
                .Select(profile => profile.DisplayName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var accessToken = _authTokenService.CreateAccessToken(user);

        var newRefreshToken = _authTokenService.CreateRefreshToken();
        var newRefreshTokenHash = _authTokenService.HashRefreshToken(newRefreshToken);
        var newRefreshTokenExpiresAt = _authTokenService.GetRefreshTokenExpiresAt(now);

        oldRefreshToken.Revoke(now, newRefreshTokenHash);

        _dbContext.RefreshTokens.Add(new RefreshToken(
            id: Guid.NewGuid(),
            userId: user.Id,
            tokenHash: newRefreshTokenHash,
            createdAt: now,
            expiresAt: newRefreshTokenExpiresAt));

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AuthResult(
            user.Id,
            user.Email,
            user.Role,
            displayName,
            accessToken.Token,
            accessToken.ExpiresAt,
            newRefreshToken,
            newRefreshTokenExpiresAt);
    }
}
