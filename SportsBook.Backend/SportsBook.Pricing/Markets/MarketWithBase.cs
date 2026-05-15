using SportsBook.Pricing.Abstractions;
using SportsBook.Pricing.Enums;
using SportsBook.Pricing.Helpers;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Markets;

public sealed record MarketWithBase : IMarket
{
    public MarketType Type { get; }
    public MarketBase Base { get; }
    public IReadOnlyList<Selection> Selections { get; }

    public MarketWithBase(
        MarketType type,
        MarketBase marketBase,
        IReadOnlyList<Selection> selections)
    {
        MarketValidator.ValidateWithBase(type, selections);

        Type = type;
        Base = marketBase;
        Selections = selections;
    }
}
