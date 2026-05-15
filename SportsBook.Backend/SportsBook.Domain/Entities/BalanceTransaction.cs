using SportsBook.Domain.Enums;

namespace SportsBook.Domain.Entities;

public sealed class BalanceTransaction
{
    public Guid Id { get; private set; }

    public Guid WalletId { get; private set; }
    public Guid UserId { get; private set; }

    public Guid? BetId { get; private set; }

    public BalanceTransactionType Type { get; private set; }

    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private BalanceTransaction()
    {
    }

    public BalanceTransaction(
        Guid id,
        Guid walletId,
        Guid userId,
        Guid? betId,
        BalanceTransactionType type,
        decimal amount,
        decimal balanceAfter,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Transaction id cannot be empty.", nameof(id));

        if (walletId == Guid.Empty)
            throw new ArgumentException("Wallet id cannot be empty.", nameof(walletId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));

        if (amount == 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Transaction amount cannot be zero.");

        if (balanceAfter < 0)
            throw new ArgumentOutOfRangeException(nameof(balanceAfter), "Balance after transaction cannot be negative.");

        Id = id;
        WalletId = walletId;
        UserId = userId;
        BetId = betId;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        CreatedAt = createdAt;
    }
}
