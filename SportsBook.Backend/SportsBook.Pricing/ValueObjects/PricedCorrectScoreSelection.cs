using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;
using SportsBook.Pricing.Abstractions;
using SportsBook.Pricing.Enums;

namespace SportsBook.Pricing.ValueObjects;

public sealed record PricedCorrectScoreSelection(
    Score Score,
    Probability Probability,
    Odds Odds
) : ISelection
{
    public SelectionCode Code => SelectionCode.ExactScore;
}
