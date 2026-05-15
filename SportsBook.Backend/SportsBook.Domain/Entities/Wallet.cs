using SportsBook.Domain.Enums;

namespace SportsBook.Domain.Entities;

public sealed class Wallet
{
    private readonly List<BalanceTransaction> _transactions = new();

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    public decimal Balance { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<BalanceTransaction> Transactions => _transactions.AsReadOnly();

    private Wallet()
    {
    }

    public Wallet(
        Guid id,
        Guid userId,
        decimal initialBalance,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Wallet id cannot be empty.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));

        if (initialBalance < 0)
            throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial balance cannot be negative.");

        Id = id;
        UserId = userId;
        Balance = initialBalance;
        CreatedAt = createdAt;
    }

    public BalanceTransaction Deposit(
        Guid transactionId,
        decimal amount,
        DateTimeOffset createdAt)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit amount must be positive.");

        return AddTransaction(
            transactionId,
            betId: null,
            BalanceTransactionType.Deposit,
            amount,
            createdAt);
    }

    public BalanceTransaction WithdrawStake(
        Guid transactionId,
        Guid betId,
        decimal stake,
        DateTimeOffset createdAt)
    {
        if (betId == Guid.Empty)
            throw new ArgumentException("Bet id cannot be empty.", nameof(betId));

        if (stake <= 0)
            throw new ArgumentOutOfRangeException(nameof(stake), "Stake must be positive.");

        if (Balance < stake)
            throw new InvalidOperationException("Insufficient balance.");

        return AddTransaction(
            transactionId,
            betId,
            BalanceTransactionType.BetStake,
            -stake,
            createdAt);
    }

    public BalanceTransaction CreditBetWin(
        Guid transactionId,
        Guid betId,
        decimal payout,
        DateTimeOffset createdAt)
    {
        if (betId == Guid.Empty)
            throw new ArgumentException("Bet id cannot be empty.", nameof(betId));

        if (payout <= 0)
            throw new ArgumentOutOfRangeException(nameof(payout), "Payout must be positive.");

        return AddTransaction(
            transactionId,
            betId,
            BalanceTransactionType.BetWin,
            payout,
            createdAt);
    }

    public BalanceTransaction RefundBet(
        Guid transactionId,
        Guid betId,
        decimal amount,
        DateTimeOffset createdAt)
    {
        if (betId == Guid.Empty)
            throw new ArgumentException("Bet id cannot be empty.", nameof(betId));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Refund amount must be positive.");

        return AddTransaction(
            transactionId,
            betId,
            BalanceTransactionType.BetRefund,
            amount,
            createdAt);
    }

    private BalanceTransaction AddTransaction(
        Guid transactionId,
        Guid? betId,
        BalanceTransactionType type,
        decimal amount,
        DateTimeOffset createdAt)
    {
        var newBalance = Balance + amount;

        if (newBalance < 0)
            throw new InvalidOperationException("Wallet balance cannot become negative.");

        Balance = newBalance;
        UpdatedAt = createdAt;

        var transaction = new BalanceTransaction(
            transactionId,
            walletId: Id,
            userId: UserId,
            betId,
            type,
            amount,
            balanceAfter: Balance,
            createdAt);

        _transactions.Add(transaction);

        return transaction;
    }
}
