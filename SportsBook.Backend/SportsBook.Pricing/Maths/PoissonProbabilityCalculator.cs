using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Maths;

public static class PoissonProbabilityCalculator
{
    /// <summary>
    /// Считает вероятность того, что пуассоновская случайная величина
    /// с заданной лямбдой будет ровно равна указанному значению.
    /// </summary>
    /// <param name="value">Целевое неотрицательное целое значение.</param>
    /// <param name="lambda">Математическое ожидание распределения.</param>
    /// <returns>Вероятность события X = value.</returns>
    public static Probability Exact(int value, double lambda)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfNegative(lambda);

        if (!double.IsFinite(lambda))
            throw new ArgumentOutOfRangeException(nameof(lambda), "Lambda must be finite.");

        if (lambda == 0d)
            return new Probability(value == 0 ? 1d : 0d);

        var probability = Math.Exp(-lambda);

        for (var i = 1; i <= value; i++)
            probability *= lambda / i;

        return new Probability(probability);
    }

    /// <summary>
    /// Считает накопленную вероятность того, что пуассоновская случайная величина
    /// с заданной лямбдой будет меньше или равна указанному значению.
    /// </summary>
    /// <param name="value">Верхняя граница включительно.</param>
    /// <param name="lambda">Математическое ожидание распределения.</param>
    /// <returns>Вероятность события X &lt;= value.</returns>
    public static Probability LessThanOrEqual(int value, double lambda)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfNegative(lambda);

        if (!double.IsFinite(lambda))
            throw new ArgumentOutOfRangeException(nameof(lambda), "Lambda must be finite.");

        if (lambda == 0d)
            return new Probability(1d);

        var sum = 0d;
        var probability = Math.Exp(-lambda);

        for (var i = 0; i <= value; i++)
        {
            if (i > 0)
                probability *= lambda / i;

            sum += probability;
        }

        return new Probability(sum);
    }

    /// <summary>
    /// Считает вероятность того, что пуассоновская случайная величина
    /// с заданной лямбдой будет больше указанного значения.
    /// </summary>
    /// <param name="value">Нижняя граница не включительно.</param>
    /// <param name="lambda">Математическое ожидание распределения.</param>
    /// <returns>Вероятность события X &gt; value.</returns>
    public static Probability GreaterThan(int value, double lambda) =>
        LessThanOrEqual(value, lambda).Inverse();

    /// <summary>
    /// Считает вероятность того, что пуассоновская случайная величина
    /// с заданной лямбдой будет меньше указанного значения.
    /// </summary>
    /// <param name="value">Верхняя граница не включительно.</param>
    /// <param name="lambda">Математическое ожидание распределения.</param>
    /// <returns>Вероятность события X &lt; value.</returns>
    public static Probability LessThan(int value, double lambda)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        return value == 0
            ? new Probability(0d)
            : LessThanOrEqual(value - 1, lambda);
    }

    /// <summary>
    /// Считает вероятность того, что пуассоновская случайная величина
    /// с заданной лямбдой будет больше или равна указанному значению.
    /// </summary>
    /// <param name="value">Нижняя граница включительно.</param>
    /// <param name="lambda">Математическое ожидание распределения.</param>
    /// <returns>Вероятность события X &gt;= value.</returns>
    public static Probability GreaterThanOrEqual(int value, double lambda) =>
        LessThan(value, lambda).Inverse();

    /// <summary>
    /// Считает вероятность исхода Over для рынка с базой.
    /// </summary>
    /// <param name="marketBase">База рынка.</param>
    /// <param name="lambda">Математическое ожидание распределения.</param>
    /// <returns>Вероятность исхода Over.</returns>
    public static Probability Over(MarketBase marketBase, double lambda)
    {
        var threshold = (int)Math.Floor(marketBase.Value);

        return GreaterThan(threshold, lambda);
    }

    /// <summary>
    /// Считает вероятность исхода Under для рынка с базой.
    /// </summary>
    /// <param name="marketBase">База рынка.</param>
    /// <param name="lambda">Математическое ожидание распределения.</param>
    /// <returns>Вероятность исхода Under.</returns>
    public static Probability Under(MarketBase marketBase, double lambda)
    {
        var threshold = (int)Math.Floor(marketBase.Value);

        return LessThanOrEqual(threshold, lambda);
    }
}
