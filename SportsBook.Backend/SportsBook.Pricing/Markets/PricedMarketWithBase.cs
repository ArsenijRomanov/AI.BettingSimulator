using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;
using SportsBook.Pricing.Abstractions;
using SportsBook.Pricing.Enums;
using SportsBook.Pricing.Helpers;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Markets;

public sealed record PricedMarketWithBase : IMarket
{
    public MarketType Type { get; }
    public MarketBase Base { get; }
    public IReadOnlyList<PricedSelection> Selections { get; }

    public PricedMarketWithBase(
        MarketType type,
        MarketBase marketBase,
        IReadOnlyList<PricedSelection> selections)
    {
        MarketValidator.ValidateWithBase(type, selections);

        Type = type;
        Base = marketBase;
        Selections = selections;
    }
}
