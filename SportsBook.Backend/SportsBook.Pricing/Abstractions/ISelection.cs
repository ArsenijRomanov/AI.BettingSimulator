using SportsBook.Pricing.Enums;
using SportsBook.Pricing.ValueObjects;

namespace SportsBook.Pricing.Abstractions;

public interface ISelection
{
    SelectionCode Code { get; }
    Probability Probability { get; }
    Odds Odds { get; }
}
