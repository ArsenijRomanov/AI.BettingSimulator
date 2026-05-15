using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Application.Common;

public sealed record MarketRequestDto(
    MarketType Type,
    MarketBase? Base,
    double Margin,
    IReadOnlyList<Score>? ExactScores = null);