namespace SportsBook.Domain.Enums;

public enum MatchStatus : byte
{
    Draft = 1,
    Open = 2,
    Suspended = 3,
    Closed = 4,
    Settled = 5,
    Cancelled = 6
}
