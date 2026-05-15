using SportsBook.Domain.Entities;

namespace SportsBook.Application.Abstractions;

public interface IFinancialLockService
{
    Task<Match?> LockMatchWithMarketsForUpdateAsync(
        Guid matchId,
        CancellationToken cancellationToken = default);

    Task<Wallet?> LockWalletByUserIdForUpdateAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, Wallet>> LockWalletsByUserIdsForUpdateAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Bet>> LockAcceptedBetsForMatchForUpdateAsync(
        Guid matchId,
        CancellationToken cancellationToken = default);
}
