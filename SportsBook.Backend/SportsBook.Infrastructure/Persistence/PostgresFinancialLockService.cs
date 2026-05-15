using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Entities;

namespace SportsBook.Infrastructure.Persistence;

public sealed class PostgresFinancialLockService : IFinancialLockService
{
    private readonly SportsBookDbContext _dbContext;

    public PostgresFinancialLockService(SportsBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Match?> LockMatchWithMarketsForUpdateAsync(
        Guid matchId,
        CancellationToken cancellationToken = default)
    {
        var match = await _dbContext.Matches
            .FromSqlInterpolated($"""
                SELECT *
                FROM matches
                WHERE "Id" = {matchId}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);

        if (match is null)
            return null;

        await _dbContext.Entry(match)
            .Collection(match => match.Markets)
            .Query()
            .Include(market => market.Selections)
            .LoadAsync(cancellationToken);

        return match;
    }

    public async Task<Wallet?> LockWalletByUserIdForUpdateAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wallets
            .FromSqlInterpolated($"""
                SELECT *
                FROM wallets
                WHERE "UserId" = {userId}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, Wallet>> LockWalletsByUserIdsForUpdateAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var uniqueUserIds = userIds
            .Distinct()
            .OrderBy(userId => userId)
            .ToList();

        var wallets = new Dictionary<Guid, Wallet>();

        foreach (var userId in uniqueUserIds)
        {
            var wallet = await LockWalletByUserIdForUpdateAsync(
                userId,
                cancellationToken);

            if (wallet is null)
                throw new InvalidOperationException($"Wallet for user {userId} was not found.");

            wallets.Add(userId, wallet);
        }

        return wallets;
    }

    public async Task<IReadOnlyList<Bet>> LockAcceptedBetsForMatchForUpdateAsync(
        Guid matchId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bets
            .FromSqlInterpolated($"""
                SELECT *
                FROM bets
                WHERE "MatchId" = {matchId}
                  AND "Status" = 'Accepted'
                FOR UPDATE
                """)
            .ToListAsync(cancellationToken);
    }
}
