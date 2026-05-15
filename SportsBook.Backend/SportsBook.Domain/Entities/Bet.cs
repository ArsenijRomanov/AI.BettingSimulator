using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Domain.Entities;

public sealed class Bet
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }
    public Guid MatchId { get; private set; }
    public Guid MarketId { get; private set; }
    public Guid SelectionId { get; private set; }

    public decimal Stake { get; private set; }

    public Odds OddsSnapshot { get; private set; }
    public int OddsVersionSnapshot { get; private set; }

    public BetStatus Status { get; private set; }

    public decimal PotentialPayout { get; private set; }
    public decimal? ActualPayout { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SettledAt { get; private set; }

    private Bet()
    {
    }

    public Bet(
        Guid id,
        Guid userId,
        Guid matchId,
        Guid marketId,
        Guid selectionId,
        decimal stake,
        Odds oddsSnapshot,
        int oddsVersionSnapshot,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Bet id cannot be empty.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));

        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));

        if (marketId == Guid.Empty)
            throw new ArgumentException("Market id cannot be empty.", nameof(marketId));

        if (selectionId == Guid.Empty)
            throw new ArgumentException("Selection id cannot be empty.", nameof(selectionId));

        if (stake <= 0)
            throw new ArgumentOutOfRangeException(nameof(stake), "Stake must be positive.");

        if (oddsVersionSnapshot < 1)
            throw new ArgumentOutOfRangeException(nameof(oddsVersionSnapshot), "Odds version must be positive.");

        Id = id;
        UserId = userId;
        MatchId = matchId;
        MarketId = marketId;
        SelectionId = selectionId;

        Stake = stake;

        OddsSnapshot = oddsSnapshot;
        OddsVersionSnapshot = oddsVersionSnapshot;

        Status = BetStatus.Accepted;

        PotentialPayout = CalculatePayout(stake, oddsSnapshot);
        CreatedAt = createdAt;
    }

    public void MarkWon(DateTimeOffset settledAt)
    {
        EnsureAccepted();

        Status = BetStatus.Won;
        ActualPayout = PotentialPayout;
        SettledAt = settledAt;
    }

    public void MarkLost(DateTimeOffset settledAt)
    {
        EnsureAccepted();

        Status = BetStatus.Lost;
        ActualPayout = 0m;
        SettledAt = settledAt;
    }

    public void Refund(DateTimeOffset settledAt)
    {
        EnsureAccepted();

        Status = BetStatus.Refunded;
        ActualPayout = Stake;
        SettledAt = settledAt;
    }

    private void EnsureAccepted()
    {
        if (Status != BetStatus.Accepted)
            throw new InvalidOperationException("Only accepted bet can be changed.");
    }

    private static decimal CalculatePayout(decimal stake, Odds odds)
    {
        return decimal.Round(stake * Convert.ToDecimal(odds.Value), 2);
    }
}
