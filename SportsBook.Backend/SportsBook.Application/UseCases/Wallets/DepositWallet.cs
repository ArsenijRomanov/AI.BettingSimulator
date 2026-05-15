using SportsBook.Application.Abstractions;

namespace SportsBook.Application.UseCases.Wallets;

public sealed record DepositWalletCommand(
    Guid UserId,
    decimal Amount);

public sealed record DepositWalletResult(
    Guid UserId,
    decimal Amount,
    decimal BalanceAfter,
    Guid TransactionId);

public sealed class DepositWalletHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IFinancialLockService _financialLockService;

    public DepositWalletHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        IFinancialLockService financialLockService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _financialLockService = financialLockService;
    }

    public async Task<DepositWalletResult> Handle(
        DepositWalletCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var wallet = await _financialLockService.LockWalletByUserIdForUpdateAsync(
            command.UserId,
            cancellationToken);

        if (wallet is null)
            throw new InvalidOperationException("Wallet was not found.");

        var balanceTransaction = wallet.Deposit(
            transactionId: Guid.NewGuid(),
            amount: command.Amount,
            createdAt: now);

        _dbContext.BalanceTransactions.Add(balanceTransaction);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new DepositWalletResult(
            wallet.UserId,
            command.Amount,
            wallet.Balance,
            balanceTransaction.Id);
    }
}
