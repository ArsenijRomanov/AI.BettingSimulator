using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Application.Common;

public sealed record MarketDto(
    Guid MarketId,
    MarketType Type,
    MarketBase? Base,
    double Margin,
    IReadOnlyList<SelectionDto> Selections);
    