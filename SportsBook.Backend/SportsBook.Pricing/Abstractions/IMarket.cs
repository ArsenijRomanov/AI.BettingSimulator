using SportsBook.Pricing.Enums;

namespace SportsBook.Pricing.Abstractions;

public interface IMarket
{
    MarketType Type { get; }
}
