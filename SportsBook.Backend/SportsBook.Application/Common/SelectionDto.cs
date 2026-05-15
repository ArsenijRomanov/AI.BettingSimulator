using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Application.Common;

public sealed record SelectionDto(
    Guid SelectionId,
    SelectionCode Code,
    string Name,
    Probability FairProbability,
    Odds FairOdds,
    Odds Odds,
    int OddsVersion,
    Score? ExactScore);
    