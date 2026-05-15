using SportsBook.Application.Abstractions;

namespace SportsBook.Application.UseCases.Auth;

public sealed record LogoutCommand(
    string RefreshToken);

public sealed record LogoutResult(
    bool Success);

public sealed class LogoutHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IAuthTokenService _authTokenService;
    private readonly IAuthLockService _authLockService;

    public LogoutHandler(
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

    public async Task<LogoutResult> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
            return new LogoutResult(true);

        var now = _clock.UtcNow;
        var tokenHash = _authTokenService.HashRefreshToken(command.RefreshToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var refreshToken = await _authLockService.LockRefreshTokenByHashForUpdateAsync(
            tokenHash,
            cancellationToken);

        refreshToken?.Revoke(now);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new LogoutResult(true);
    }
}
