namespace SportsBook.Pricing.Exceptions;

public static class PricingErrorCodes
{
    public const string MarginTooHigh = "MarginTooHigh";

    public const string MarketBaseOutOfMatrix = "MarketBaseOutOfMatrix";
    public const string ScoreOutOfMatrix = "ScoreOutOfMatrix";
    public const string UnsupportedMarketType = "UnsupportedMarketType";

    public const string LambdaCalculationFailed = "LambdaCalculationFailed";
    public const string InvalidSourceMarket = "InvalidSourceMarket";
}
