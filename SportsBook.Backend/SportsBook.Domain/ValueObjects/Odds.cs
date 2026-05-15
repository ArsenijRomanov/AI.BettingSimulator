using System.Globalization;

namespace SportsBook.Domain.ValueObjects;

public readonly record struct Odds
{
    public double Value { get; }

    public Odds(double value)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Odds must be finite.");

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 1d);

        Value = value;
    }

    public Probability ToProbability() => new(1d / Value);

    public override string ToString() =>
        Value.ToString("0.####", CultureInfo.InvariantCulture);

    public static explicit operator double(Odds odds) => odds.Value;

    public static double operator *(double value, Odds odds) =>
        value * odds.Value;

    public static double operator *(Odds odds, double value) =>
        odds.Value * value;
}
