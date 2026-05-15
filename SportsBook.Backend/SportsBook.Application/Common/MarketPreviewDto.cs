using SportsBook.Domain.Enums;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Application.Common;

public sealed record MarketPreviewDto(
    MarketType Type,
    MarketBase? Base,
    IReadOnlyList<SelectionPreviewDto> Selections);
    