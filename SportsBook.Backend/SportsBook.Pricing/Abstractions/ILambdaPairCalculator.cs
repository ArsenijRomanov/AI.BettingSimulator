using SportsBook.Pricing.Markets;

namespace SportsBook.Pricing.Abstractions;

public interface ILambdaPairCalculator
{
    /// <summary>
    /// Восстанавливает индивидуальные лямбды команд по рынку тотала и рынку форы.
    /// </summary>
    /// <param name="totalMarket">Рынок общего тотала.</param>
    /// <param name="handicapMarket">Рынок форы.</param>
    /// <returns>Кортеж с индивидуальными лямбдами Home и Away.</returns>
    (double Home, double Away) Calculate(
        MarketWithBase totalMarket,
        MarketWithBase handicapMarket);

    /// <summary>
    /// Восстанавливает общую лямбду матча по рынку общего тотала.
    /// </summary>
    /// <param name="totalMarket">Рынок общего тотала.</param>
    /// <returns>Общая лямбда матча.</returns>
    double CalculateTotalLambda(MarketWithBase totalMarket);

    /// <summary>
    /// Восстанавливает индивидуальные лямбды команд по общей лямбде матча и рынку форы.
    /// </summary>
    /// <param name="totalLambda">Общая лямбда матча.</param>
    /// <param name="handicapMarket">Рынок форы.</param>
    /// <returns>Кортеж с индивидуальными лямбдами Home и Away.</returns>
    (double Home, double Away) CalculateByTotalLambdaAndHandicap(
        double totalLambda,
        MarketWithBase handicapMarket);
}
