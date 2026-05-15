using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Domain.Entities;

public sealed class Market
{
    private readonly List<Selection> _selections = new();

    public Guid Id { get; private set; }
    public Guid MatchId { get; private set; }

    public MarketType Type { get; private set; }
    public MarketBase? Base { get; private set; }

    public double Margin { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Selection> Selections => _selections.AsReadOnly();

    private Market()
    {
    }

    public Market(
        Guid id,
        Guid matchId,
        MarketType type,
        MarketBase? @base,
        double margin)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Market id cannot be empty.", nameof(id));

        if (matchId == Guid.Empty)
            throw new ArgumentException("Match id cannot be empty.", nameof(matchId));

        if (!double.IsFinite(margin) || margin < 0)
            throw new ArgumentOutOfRangeException(nameof(margin), "Margin cannot be negative.");

        Id = id;
        MatchId = matchId;
        Type = type;
        Base = @base;
        Margin = margin;

        // Draft-market не должен быть активным.
        IsActive = false;

        ValidateBase();
    }

    public void AddSelection(Selection selection)
    {
        if (selection.MarketId != Id)
            throw new InvalidOperationException("Selection belongs to another market.");

        ValidateSelectionFitsMarket(selection);
        EnsureSelectionNotDuplicated(selection);

        _selections.Add(selection);
    }

    public void EnsureReadyToOpen()
    {
        ValidateBase();

        switch (Type)
        {
            case MarketType.HomeDrawAway:
                EnsureSelectionCodes(
                    SelectionCode.Home,
                    SelectionCode.Draw,
                    SelectionCode.Away);
                return;

            case MarketType.Total:
            case MarketType.HomeTotal:
            case MarketType.AwayTotal:
                EnsureSelectionCodes(
                    SelectionCode.Over,
                    SelectionCode.Under);
                return;

            case MarketType.Handicap:
                EnsureSelectionCodes(
                    SelectionCode.Home,
                    SelectionCode.Away);
                return;

            case MarketType.CorrectScore:
                if (_selections.Count == 0)
                    throw new InvalidOperationException("CorrectScore market must contain at least one exact score selection.");

                if (_selections.Any(selection => selection.Code != SelectionCode.ExactScore || selection.ExactScore is null))
                    throw new InvalidOperationException("CorrectScore market contains invalid selections.");

                return;

            default:
                throw new InvalidOperationException($"Unsupported market type {Type}.");
        }
    }

    public void Activate()
    {
        EnsureReadyToOpen();

        IsActive = true;

        foreach (var selection in _selections)
            selection.Activate();
    }

    public void Deactivate()
    {
        IsActive = false;

        foreach (var selection in _selections)
            selection.Deactivate();
    }

    public Selection GetSelection(Guid selectionId)
    {
        return _selections.FirstOrDefault(selection => selection.Id == selectionId)
               ?? throw new InvalidOperationException("Selection not found in this market.");
    }

    public void EnsureCanAcceptBets()
    {
        if (!IsActive)
            throw new InvalidOperationException("Market is not active.");
    }

    private void ValidateBase()
    {
        var requiresBase = Type is
            MarketType.Total or
            MarketType.HomeTotal or
            MarketType.AwayTotal or
            MarketType.Handicap;

        if (requiresBase && Base is null)
            throw new InvalidOperationException($"Market type {Type} requires base.");

        var mustNotHaveBase = Type is
            MarketType.HomeDrawAway or
            MarketType.CorrectScore;

        if (mustNotHaveBase && Base is not null)
            throw new InvalidOperationException($"Market type {Type} must not have base.");
    }

    private void ValidateSelectionFitsMarket(Selection selection)
    {
        var isValid = Type switch
        {
            MarketType.HomeDrawAway =>
                selection.Code is SelectionCode.Home or SelectionCode.Draw or SelectionCode.Away &&
                selection.ExactScore is null,

            MarketType.Total =>
                selection.Code is SelectionCode.Over or SelectionCode.Under &&
                selection.ExactScore is null,

            MarketType.HomeTotal =>
                selection.Code is SelectionCode.Over or SelectionCode.Under &&
                selection.ExactScore is null,

            MarketType.AwayTotal =>
                selection.Code is SelectionCode.Over or SelectionCode.Under &&
                selection.ExactScore is null,

            MarketType.Handicap =>
                selection.Code is SelectionCode.Home or SelectionCode.Away &&
                selection.ExactScore is null,

            MarketType.CorrectScore =>
                selection.Code == SelectionCode.ExactScore &&
                selection.ExactScore is not null,

            _ => false
        };

        if (!isValid)
            throw new InvalidOperationException($"Selection {selection.Code} does not fit market {Type}.");
    }

    private void EnsureSelectionNotDuplicated(Selection selection)
    {
        var duplicateExists = Type == MarketType.CorrectScore
            ? _selections.Any(existing => existing.ExactScore == selection.ExactScore)
            : _selections.Any(existing => existing.Code == selection.Code);

        if (duplicateExists)
            throw new InvalidOperationException("Selection already exists in this market.");
    }

    private void EnsureSelectionCodes(params SelectionCode[] requiredCodes)
    {
        foreach (var code in requiredCodes)
        {
            if (_selections.All(selection => selection.Code != code))
                throw new InvalidOperationException($"Market {Type} is missing selection {code}.");
        }

        if (_selections.Count != requiredCodes.Length)
            throw new InvalidOperationException($"Market {Type} has invalid selections count.");
    }
}
