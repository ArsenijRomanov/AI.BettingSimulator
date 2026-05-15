using SportsBook.Domain.Enums;
using SportsBook.Pricing.Enums;

namespace SportsBook.Pricing.Abstractions;

public interface IMarket
{
    MarketType Type { get; }
}
