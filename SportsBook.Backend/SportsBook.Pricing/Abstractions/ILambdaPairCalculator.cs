using SportsBook.Pricing.Markets;

namespace SportsBook.Pricing.Abstractions;

public interface ILambdaPairCalculator
{
    /// <summary>
    /// Восстанавливает индивидуальные лямбды команд по рынку тотала и рынку форы.
    /// </summary>
    /// <param name="totalPricedMarket">Рынок общего тотала.</param>
    /// <param name="handicapPricedMarket">Рынок форы.</param>
    /// <returns>Кортеж с индивидуальными лямбдами Home и Away.</returns>
    (double Home, double Away) Calculate(
        PricedMarketWithBase totalPricedMarket,
        PricedMarketWithBase handicapPricedMarket);

    /// <summary>
    /// Восстанавливает общую лямбду матча по рынку общего тотала.
    /// </summary>
    /// <param name="totalPricedMarket">Рынок общего тотала.</param>
    /// <returns>Общая лямбда матча.</returns>
    double CalculateTotalLambda(PricedMarketWithBase totalPricedMarket);

    /// <summary>
    /// Восстанавливает индивидуальные лямбды команд по общей лямбде матча и рынку форы.
    /// </summary>
    /// <param name="totalLambda">Общая лямбда матча.</param>
    /// <param name="handicapPricedMarket">Рынок форы.</param>
    /// <returns>Кортеж с индивидуальными лямбдами Home и Away.</returns>
    (double Home, double Away) CalculateByTotalLambdaAndHandicap(
        double totalLambda,
        PricedMarketWithBase handicapPricedMarket);
}
