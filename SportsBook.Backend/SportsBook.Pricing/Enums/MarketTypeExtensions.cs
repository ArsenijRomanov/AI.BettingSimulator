namespace SportsBook.Pricing.Enums;

public static class MarketTypeExtensions
{
    public static bool RequiresBase(this MarketType type) =>
        type is
            MarketType.Total or
            MarketType.HomeTotal or
            MarketType.AwayTotal or
            MarketType.Handicap;
}
