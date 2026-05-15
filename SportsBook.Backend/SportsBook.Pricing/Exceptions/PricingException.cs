namespace SportsBook.Pricing.Exceptions;

public sealed class PricingException : Exception
{
    public string Code { get; }

    public PricingException(
        string code,
        string message)
        : base(message)
    {
        Code = code;
    }

    public PricingException(
        string code,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}
