using SportsBook.Application.Abstractions;
using SportsBook.Domain.Entities;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Application.UseCases.Bets;

public sealed record PlaceBetCommand(
    Guid UserId,
    Guid MatchId,
    Guid MarketId,
    Guid SelectionId,
    decimal Stake,
    double ExpectedOdds,
    int OddsVersion);

public sealed record PlaceBetResult(
    Guid BetId,
    string Status,
    Guid MatchId,
    string Selection,
    decimal Stake,
    double Odds,
    decimal PotentialPayout,
    decimal BalanceAfterBet);

public sealed class PlaceBetHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IFinancialLockService _financialLockService;

    public PlaceBetHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        IFinancialLockService financialLockService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _financialLockService = financialLockService;
    }

    public async Task<PlaceBetResult> Handle(
        PlaceBetCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var match = await _financialLockService.LockMatchWithMarketsForUpdateAsync(
            command.MatchId,
            cancellationToken);

        if (match is null)
            throw new InvalidOperationException("Match was not found.");

        var market = match.Markets.FirstOrDefault(market => market.Id == command.MarketId)
            ?? throw new InvalidOperationException("Market was not found.");

        var selection = market.Selections.FirstOrDefault(selection => selection.Id == command.SelectionId)
            ?? throw new InvalidOperationException("Selection was not found.");

        var wallet = await _financialLockService.LockWalletByUserIdForUpdateAsync(
            command.UserId,
            cancellationToken);

        if (wallet is null)
            throw new InvalidOperationException("Wallet was not found.");

        match.EnsureCanAcceptBets(now);
        market.EnsureCanAcceptBets();

        selection.EnsureCanAcceptBets(
            expectedOdds: new Odds(command.ExpectedOdds),
            expectedOddsVersion: command.OddsVersion);

        var bet = new Bet(
            id: Guid.NewGuid(),
            userId: command.UserId,
            matchId: match.Id,
            marketId: market.Id,
            selectionId: selection.Id,
            stake: command.Stake,
            oddsSnapshot: selection.Odds,
            oddsVersionSnapshot: selection.OddsVersion,
            createdAt: now);

        var balanceTransaction = wallet.WithdrawStake(
            transactionId: Guid.NewGuid(),
            betId: bet.Id,
            stake: bet.Stake,
            createdAt: now);

        _dbContext.Bets.Add(bet);
        _dbContext.BalanceTransactions.Add(balanceTransaction);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PlaceBetResult(
            bet.Id,
            bet.Status.ToString(),
            bet.MatchId,
            selection.Name,
            bet.Stake,
            bet.OddsSnapshot.Value,
            bet.PotentialPayout,
            wallet.Balance);
    }
}
