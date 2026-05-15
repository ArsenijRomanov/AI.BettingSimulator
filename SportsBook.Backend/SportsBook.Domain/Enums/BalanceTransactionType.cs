namespace SportsBook.Domain.Enums;

public enum BalanceTransactionType : byte
{
    Deposit = 1,
    BetStake = 2,
    BetWin = 3,
    BetRefund = 4
}
