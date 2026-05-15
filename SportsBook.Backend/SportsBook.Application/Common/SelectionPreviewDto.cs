using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Application.Common;

public sealed record SelectionPreviewDto(
    SelectionCode Code,
    string Name,
    Probability FairProbability,
    Odds FairOdds,
    Score? ExactScore);
    