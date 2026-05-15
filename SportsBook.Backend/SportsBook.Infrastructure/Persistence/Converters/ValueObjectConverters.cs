using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Infrastructure.Persistence.Converters;

internal static class ValueObjectConverters
{
    public static readonly ValueConverter<MarketBase, double> MarketBaseConverter = new(
        marketBase => marketBase.Value,
        value => new MarketBase(value));

    public static readonly ValueConverter<MarketBase?, double?> NullableMarketBaseConverter = new(
        marketBase => marketBase.HasValue
            ? marketBase.Value.Value
            : null,
        value => value.HasValue
            ? new MarketBase(value.Value)
            : null);

    public static readonly ValueConverter<Odds, double> OddsConverter = new(
        odds => odds.Value,
        value => new Odds(value));

    public static readonly ValueConverter<Probability, double> ProbabilityConverter = new(
        probability => probability.Value,
        value => new Probability(value));

    public static readonly ValueConverter<Score?, string?> NullableScoreConverter = new(
        score => ScoreToString(score),
        value => StringToScore(value));

    private static string? ScoreToString(Score? score)
    {
        if (score is null)
            return null;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{score.Value.Home}:{score.Value.Away}");
    }

    private static Score? StringToScore(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var parts = value.Split(':', 2);

        if (parts.Length != 2)
            throw new InvalidOperationException($"Invalid score value '{value}'.");

        var home = int.Parse(parts[0], CultureInfo.InvariantCulture);
        var away = int.Parse(parts[1], CultureInfo.InvariantCulture);

        return new Score(home, away);
    }
}
