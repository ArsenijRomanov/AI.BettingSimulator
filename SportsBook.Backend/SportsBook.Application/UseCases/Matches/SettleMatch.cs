using SportsBook.Application.Abstractions;
using SportsBook.Domain.Enums;
using SportsBook.Domain.Services;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Application.UseCases.Matches;

public sealed record SettleMatchCommand(
    Guid MatchId,
    int HomeScore,
    int AwayScore);

public sealed record SettleMatchResult(
    Guid MatchId,
    MatchStatus Status,
    int HomeScore,
    int AwayScore,
    int TotalBets,
    int WonBets,
    int LostBets,
    decimal TotalPaid);

public sealed class SettleMatchHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly SelectionSettlementService _selectionSettlementService;
    private readonly IFinancialLockService _financialLockService;

    public SettleMatchHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        SelectionSettlementService selectionSettlementService,
        IFinancialLockService financialLockService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _selectionSettlementService = selectionSettlementService;
        _financialLockService = financialLockService;
    }

    public async Task<SettleMatchResult> Handle(
        SettleMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var finalScore = new Score(command.HomeScore, command.AwayScore);

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

        var marketsById = match.Markets.ToDictionary(market => market.Id);

        var selectionsById = match.Markets
            .SelectMany(market => market.Selections)
            .ToDictionary(selection => selection.Id);

        var wonBets = 0;
        var lostBets = 0;
        var totalPaid = 0m;

        foreach (var bet in bets)
        {
            if (!marketsById.TryGetValue(bet.MarketId, out var market))
                throw new InvalidOperationException("Bet market was not found.");

            if (!selectionsById.TryGetValue(bet.SelectionId, out var selection))
                throw new InvalidOperationException("Bet selection was not found.");

            var isWinning = _selectionSettlementService.IsWinningSelection(
                market,
                selection,
                finalScore);

            if (isWinning)
            {
                bet.MarkWon(now);
                wonBets++;

                if (!wallets.TryGetValue(bet.UserId, out var wallet))
                    throw new InvalidOperationException("Wallet was not found.");

                var balanceTransaction = wallet.CreditBetWin(
                    transactionId: Guid.NewGuid(),
                    betId: bet.Id,
                    payout: bet.PotentialPayout,
                    createdAt: now);

                _dbContext.BalanceTransactions.Add(balanceTransaction);

                totalPaid += bet.PotentialPayout;
            }
            else
            {
                bet.MarkLost(now);
                lostBets++;
            }
        }

        match.Settle(finalScore, now);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new SettleMatchResult(
            match.Id,
            match.Status,
            finalScore.Home,
            finalScore.Away,
            bets.Count,
            wonBets,
            lostBets,
            totalPaid);
    }
}
