using SportsBook.Pricing.Abstractions;
using SportsBook.Pricing.Enums;
using SportsBook.Pricing.Helpers;

namespace SportsBook.Pricing.Markets;

public sealed record Market<TSelection> : IMarket
    where TSelection : ISelection
{
    public MarketType Type { get; }
    public IReadOnlyList<TSelection> Selections { get; }

    public Market(
        MarketType type,
        IReadOnlyList<TSelection> selections)
    {
        MarketValidator.Validate(type, selections);

        Type = type;
        Selections = selections;
    }
}
