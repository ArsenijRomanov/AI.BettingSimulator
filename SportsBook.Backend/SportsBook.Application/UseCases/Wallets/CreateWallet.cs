using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Entities;

namespace SportsBook.Application.UseCases.Wallets;

public sealed record CreateWalletCommand(
    Guid UserId,
    decimal InitialBalance = 0m);

public sealed record CreateWalletResult(
    Guid WalletId,
    Guid UserId,
    decimal Balance);

public sealed class CreateWalletHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;

    public CreateWalletHandler(
        ISportsBookDbContext dbContext,
        IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<CreateWalletResult> Handle(
        CreateWalletCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(command.UserId));

        if (command.InitialBalance < 0)
            throw new ArgumentOutOfRangeException(
                nameof(command.InitialBalance),
                "Initial balance cannot be negative.");

        var now = _clock.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var walletExists = await _dbContext.Wallets
            .AnyAsync(wallet => wallet.UserId == command.UserId, cancellationToken);

        if (walletExists)
            throw new InvalidOperationException("Wallet already exists.");

        var wallet = new Wallet(
            id: Guid.NewGuid(),
            userId: command.UserId,
            initialBalance: 0m,
            createdAt: now);

        _dbContext.Wallets.Add(wallet);

        if (command.InitialBalance > 0)
        {
            var balanceTransaction = wallet.Deposit(
                transactionId: Guid.NewGuid(),
                amount: command.InitialBalance,
                createdAt: now);

            _dbContext.BalanceTransactions.Add(balanceTransaction);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CreateWalletResult(
            wallet.Id,
            wallet.UserId,
            wallet.Balance);
    }
}
