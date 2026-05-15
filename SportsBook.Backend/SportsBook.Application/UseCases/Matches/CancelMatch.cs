using SportsBook.Application.Abstractions;
using SportsBook.Domain.Enums;

namespace SportsBook.Application.UseCases.Matches;

public sealed record CancelMatchCommand(
    Guid MatchId,
    string? Reason);

public sealed record CancelMatchResult(
    Guid MatchId,
    MatchStatus Status,
    int RefundedBets,
    decimal TotalRefunded);

public sealed class CancelMatchHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IFinancialLockService _financialLockService;

    public CancelMatchHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        IFinancialLockService financialLockService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _financialLockService = financialLockService;
    }

    public async Task<CancelMatchResult> Handle(
        CancelMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var match = await _financialLockService.LockMatchWithMarketsForUpdateAsync(
            command.MatchId,
            cancellationToken);

        if (match is null)
            throw new InvalidOperationException("Match was not found.");

        var bets = await _financialLockService.LockAcceptedBetsForMatchForUpdateAsync(
            command.MatchId,
            cancellationToken);

        var userIds = bets
            .Select(bet => bet.UserId)
            .Distinct()
            .ToList();

        var wallets = await _financialLockService.LockWalletsByUserIdsForUpdateAsync(
            userIds,
            cancellationToken);

        var totalRefunded = 0m;

        foreach (var bet in bets)
        {
            bet.Refund(now);

            if (!wallets.TryGetValue(bet.UserId, out var wallet))
                throw new InvalidOperationException("Wallet was not found.");

            var balanceTransaction = wallet.RefundBet(
                transactionId: Guid.NewGuid(),
                betId: bet.Id,
                amount: bet.Stake,
                createdAt: now);

            _dbContext.BalanceTransactions.Add(balanceTransaction);

            totalRefunded += bet.Stake;
        }

        match.Cancel(now);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CancelMatchResult(
            match.Id,
            match.Status,
            bets.Count,
            totalRefunded);
    }
}
