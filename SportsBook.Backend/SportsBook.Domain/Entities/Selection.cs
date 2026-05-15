using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Domain.Entities;

public sealed class Selection
{
    private const double OddsCompareTolerance = 1e-9;

    public Guid Id { get; private set; }
    public Guid MarketId { get; private set; }

    public SelectionCode Code { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Probability FairProbability { get; private set; }
    public Odds FairOdds { get; private set; }

    public Odds Odds { get; private set; }
    public int OddsVersion { get; private set; }

    public bool IsActive { get; private set; }

    public Score? ExactScore { get; private set; }

    private Selection()
    {
    }

    public Selection(
        Guid id,
        Guid marketId,
        SelectionCode code,
        string name,
        Probability fairProbability,
        Odds fairOdds,
        Odds odds,
        int oddsVersion,
        Score? exactScore = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Selection id cannot be empty.", nameof(id));

        if (marketId == Guid.Empty)
            throw new ArgumentException("Market id cannot be empty.", nameof(marketId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Selection name cannot be empty.", nameof(name));

        if (oddsVersion < 1)
            throw new ArgumentOutOfRangeException(nameof(oddsVersion), "Odds version must be positive.");

        Id = id;
        MarketId = marketId;
        Code = code;
        Name = name.Trim();

        FairProbability = fairProbability;
        FairOdds = fairOdds;

        Odds = odds;
        OddsVersion = oddsVersion;

        ExactScore = exactScore;
        IsActive = false;
    }

    public void ChangeOdds(Odds newOdds)
    {
        Odds = newOdds;
        OddsVersion++;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void EnsureCanAcceptBets(Odds expectedOdds, int expectedOddsVersion)
    {
        if (!IsActive)
            throw new InvalidOperationException("Selection is not active.");

        if (!HasActualOdds(expectedOdds, expectedOddsVersion))
            throw new InvalidOperationException("Odds are outdated.");
    }

    public bool HasActualOdds(Odds expectedOdds, int expectedOddsVersion)
    {
        return OddsVersion == expectedOddsVersion &&
               Math.Abs(Odds.Value - expectedOdds.Value) <= OddsCompareTolerance;
    }
}
