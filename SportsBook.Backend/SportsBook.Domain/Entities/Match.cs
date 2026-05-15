using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Domain.Entities;

public sealed class Match
{
    private readonly List<Market> _markets = new();

    public Guid Id { get; private set; }

    public string HomeTeamName { get; private set; } = string.Empty;
    public string AwayTeamName { get; private set; } = string.Empty;
    public string Competition { get; private set; } = string.Empty;
    public string? Venue { get; private set; }

    public DateTimeOffset StartTime { get; private set; }

    public double LambdaHome { get; private set; }
    public double LambdaAway { get; private set; }

    public PricingMode PricingMode { get; private set; }
    public string? ModelVersion { get; private set; }

    public MatchStatus Status { get; private set; }

    public Score? FinalScore { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Market> Markets => _markets.AsReadOnly();

    private Match()
    {
    }

    public Match(
        Guid id,
        string homeTeamName,
        string awayTeamName,
        string competition,
        DateTimeOffset startTime,
        string? venue,
        double lambdaHome,
        double lambdaAway,
        PricingMode pricingMode,
        string? modelVersion,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(homeTeamName))
            throw new ArgumentException("Home team name cannot be empty.", nameof(homeTeamName));

        if (string.IsNullOrWhiteSpace(awayTeamName))
            throw new ArgumentException("Away team name cannot be empty.", nameof(awayTeamName));

        if (string.IsNullOrWhiteSpace(competition))
            throw new ArgumentException("Competition cannot be empty.", nameof(competition));

        if (!double.IsFinite(lambdaHome) || lambdaHome <= 0)
            throw new ArgumentOutOfRangeException(nameof(lambdaHome), "Lambda home must be positive.");

        if (!double.IsFinite(lambdaAway) || lambdaAway <= 0)
            throw new ArgumentOutOfRangeException(nameof(lambdaAway), "Lambda away must be positive.");

        Id = id;
        HomeTeamName = homeTeamName.Trim();
        AwayTeamName = awayTeamName.Trim();
        Competition = competition.Trim();
        StartTime = startTime;
        Venue = string.IsNullOrWhiteSpace(venue) ? null : venue.Trim();

        LambdaHome = lambdaHome;
        LambdaAway = lambdaAway;

        PricingMode = pricingMode;
        ModelVersion = string.IsNullOrWhiteSpace(modelVersion) ? null : modelVersion.Trim();

        Status = MatchStatus.Draft;
        CreatedAt = createdAt;
    }

    public void AddMarket(Market market, DateTimeOffset now)
    {
        EnsureDraft();

        if (market.MatchId != Id)
            throw new InvalidOperationException("Market belongs to another match.");

        var duplicateExists = _markets.Any(existing =>
            existing.Type == market.Type &&
            NullableMarketBaseEquals(existing.Base, market.Base));

        if (duplicateExists)
            throw new InvalidOperationException("Market with same type and base already exists.");

        _markets.Add(market);
        Touch(now);
    }

    public void Open(DateTimeOffset now)
    {
        EnsureDraft();
        EnsureNotStarted(now);

        if (_markets.Count == 0)
            throw new InvalidOperationException("Cannot open match without markets.");

        foreach (var market in _markets)
            market.EnsureReadyToOpen();

        foreach (var market in _markets)
            market.Activate();

        Status = MatchStatus.Open;
        Touch(now);
    }

    public void Suspend(DateTimeOffset now)
    {
        if (Status != MatchStatus.Open)
            throw new InvalidOperationException("Only open match can be suspended.");

        foreach (var market in _markets)
            market.Deactivate();

        Status = MatchStatus.Suspended;
        Touch(now);
    }

    public void Resume(DateTimeOffset now)
    {
        if (Status != MatchStatus.Suspended)
            throw new InvalidOperationException("Only suspended match can be resumed.");

        EnsureNotStarted(now);

        if (_markets.Count == 0)
            throw new InvalidOperationException("Cannot resume match without markets.");

        foreach (var market in _markets)
            market.EnsureReadyToOpen();

        foreach (var market in _markets)
            market.Activate();

        Status = MatchStatus.Open;
        Touch(now);
    }

    public void Close(DateTimeOffset now)
    {
        if (Status is not MatchStatus.Open and not MatchStatus.Suspended)
            throw new InvalidOperationException("Only open or suspended match can be closed.");

        foreach (var market in _markets)
            market.Deactivate();

        Status = MatchStatus.Closed;
        Touch(now);
    }

    public void Settle(Score finalScore, DateTimeOffset now)
    {
        if (Status == MatchStatus.Cancelled)
            throw new InvalidOperationException("Cancelled match cannot be settled.");

        if (Status == MatchStatus.Settled)
            throw new InvalidOperationException("Match is already settled.");

        FinalScore = finalScore;

        foreach (var market in _markets)
            market.Deactivate();

        Status = MatchStatus.Settled;
        Touch(now);
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status == MatchStatus.Settled)
            throw new InvalidOperationException("Settled match cannot be cancelled.");

        if (Status == MatchStatus.Cancelled)
            throw new InvalidOperationException("Match is already cancelled.");

        foreach (var market in _markets)
            market.Deactivate();

        Status = MatchStatus.Cancelled;
        Touch(now);
    }

    public void EnsureCanAcceptBets(DateTimeOffset now)
    {
        if (Status != MatchStatus.Open)
            throw new InvalidOperationException("Bets can be accepted only for open match.");

        EnsureNotStarted(now);
    }

    private void EnsureDraft()
    {
        if (Status != MatchStatus.Draft)
            throw new InvalidOperationException("This operation is allowed only for draft match.");
    }

    private void EnsureNotStarted(DateTimeOffset now)
    {
        if (StartTime <= now)
            throw new InvalidOperationException("Match has already started.");
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }

    private static bool NullableMarketBaseEquals(MarketBase? left, MarketBase? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return Math.Abs(left.Value.Value - right.Value.Value) <= 1e-9;
    }
}
