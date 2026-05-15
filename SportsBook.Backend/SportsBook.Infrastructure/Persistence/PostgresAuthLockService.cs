using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Entities;

namespace SportsBook.Infrastructure.Persistence;

public sealed class PostgresAuthLockService : IAuthLockService
{
    private readonly SportsBookDbContext _dbContext;

    public PostgresAuthLockService(SportsBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefreshToken?> LockRefreshTokenByHashForUpdateAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .FromSqlInterpolated($"""
                                  SELECT *
                                  FROM refresh_tokens
                                  WHERE "TokenHash" = {tokenHash}
                                  FOR UPDATE
                                  """)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
