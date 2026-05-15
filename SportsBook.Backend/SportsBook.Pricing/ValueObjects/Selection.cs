using SportsBook.Pricing.Abstractions;
using SportsBook.Pricing.Enums;

namespace SportsBook.Pricing.ValueObjects;

public sealed record Selection(
    SelectionCode Code,
    Probability Probability,
    Odds Odds
) : ISelection;
