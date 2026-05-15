using System.Globalization;

namespace SportsBook.Domain.ValueObjects;

public readonly record struct Probability
{
    public const double DefaultTolerance = 1e-9;

    public double Value { get; }

    public Probability(double value)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Probability must be finite.");

        ArgumentOutOfRangeException.ThrowIfLessThan(value, 0d);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 1d);

        Value = value;
    }

    public Odds ToOdds(double margin = 0d)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(margin, 0d);

        var prob = Value * (1d + margin);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(prob, 0d);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(prob, 1d);

        return new Odds(1d / prob);
    }

    public Probability Inverse() => new(1d - Value);

    public Probability Complement() => Inverse();

    public override string ToString() =>
        Value.ToString("0.########", CultureInfo.InvariantCulture);

    public static explicit operator double(Probability probability) =>
        probability.Value;

    public static Probability operator +(Probability left, Probability right) =>
        new(left.Value + right.Value);

    public static bool SumApproximatelyEqualsOne(
        Probability first,
        Probability second,
        double tolerance = DefaultTolerance) =>
        Math.Abs(first.Value + second.Value - 1d) <= tolerance;

    public static bool SumApproximatelyEqualsOne(
        Probability first,
        Probability second,
        Probability third,
        double tolerance = DefaultTolerance) =>
        Math.Abs(first.Value + second.Value + third.Value - 1d) <= tolerance;

    public static bool SumApproximatelyEqualsOne(
        Probability first,
        Probability second,
        Probability third,
        Probability fourth,
        double tolerance = DefaultTolerance) =>
        Math.Abs(first.Value + second.Value + third.Value + fourth.Value - 1d) <= tolerance;
}
