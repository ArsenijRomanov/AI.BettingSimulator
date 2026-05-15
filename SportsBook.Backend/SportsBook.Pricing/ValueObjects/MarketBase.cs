using System.Globalization;

namespace SportsBook.Pricing.ValueObjects;

public readonly record struct MarketBase
{
    private const double HalfStepTolerance = 1e-9;

    public double Value { get; }

    public MarketBase(double value)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Market base must be finite.");

        var doubled = value * 2d;
        var rounded = Math.Round(doubled);

        if (Math.Abs(doubled - rounded) > HalfStepTolerance || Math.Abs((long)rounded) % 2 != 1)
            throw new ArgumentException("Market base must end with .5.", nameof(value));

        Value = value;
    }

    public MarketBase Opposite() => new(-Value);

    public override string ToString() =>
        Value > 0
            ? $"+{Value.ToString("0.#", CultureInfo.InvariantCulture)}"
            : Value.ToString("0.#", CultureInfo.InvariantCulture);

    public static explicit operator double(MarketBase marketBase) =>
        marketBase.Value;
}
