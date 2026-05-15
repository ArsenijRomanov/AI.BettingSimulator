using SportsBook.Application.Common;
using SportsBook.Domain.Entities;
using SportsBook.Domain.ValueObjects;

namespace SportsBook.Api.Features.Common;

public sealed record ApiScoreResponse(
    int Home,
    int Away);

public sealed record ApiSelectionResponse(
    Guid SelectionId,
    string Code,
    string Name,
    double FairProbability,
    double FairOdds,
    double Odds,
    int OddsVersion,
    ApiScoreResponse? ExactScore);

public sealed record ApiMarketResponse(
    Guid MarketId,
    string Type,
    double? Base,
    double Margin,
    IReadOnlyList<ApiSelectionResponse> Selections);

public sealed record ApiSelectionPreviewResponse(
    string Code,
    string Name,
    double FairProbability,
    double FairOdds,
    ApiScoreResponse? ExactScore);

public sealed record ApiMarketPreviewResponse(
    string Type,
    double? Base,
    IReadOnlyList<ApiSelectionPreviewResponse> Selections);

public sealed record ApiMatchSummaryResponse(
    Guid MatchId,
    string Status,
    string PricingMode,
    string HomeTeamName,
    string AwayTeamName,
    string Competition,
    DateTimeOffset StartTime,
    string? Venue,
    double LambdaHome,
    double LambdaAway);

public sealed record ApiMatchDetailsResponse(
    Guid MatchId,
    string Status,
    string PricingMode,
    string HomeTeamName,
    string AwayTeamName,
    string Competition,
    DateTimeOffset StartTime,
    string? Venue,
    double LambdaHome,
    double LambdaAway,
    ApiScoreResponse? FinalScore,
    IReadOnlyList<ApiMarketResponse> Markets);

public static class ApiMappingExtensions
{
    public static ApiScoreResponse? ToApiResponse(this Score? score)
    {
        return score is null
            ? null
            : new ApiScoreResponse(score.Value.Home, score.Value.Away);
    }

    public static ApiSelectionResponse ToApiResponse(this SelectionDto selection)
    {
        return new ApiSelectionResponse(
            selection.SelectionId,
            selection.Code.ToString(),
            selection.Name,
            selection.FairProbability.Value,
            selection.FairOdds.Value,
            selection.Odds.Value,
            selection.OddsVersion,
            selection.ExactScore.ToApiResponse());
    }

    public static ApiMarketResponse ToApiResponse(this MarketDto market)
    {
        return new ApiMarketResponse(
            market.MarketId,
            market.Type.ToString(),
            market.Base?.Value,
            market.Margin,
            market.Selections.Select(selection => selection.ToApiResponse()).ToList());
    }

    public static ApiSelectionPreviewResponse ToApiResponse(this SelectionPreviewDto selection)
    {
        return new ApiSelectionPreviewResponse(
            selection.Code.ToString(),
            selection.Name,
            selection.FairProbability.Value,
            selection.FairOdds.Value,
            selection.ExactScore.ToApiResponse());
    }

    public static ApiMarketPreviewResponse ToApiResponse(this MarketPreviewDto market)
    {
        return new ApiMarketPreviewResponse(
            market.Type.ToString(),
            market.Base?.Value,
            market.Selections.Select(selection => selection.ToApiResponse()).ToList());
    }

    public static ApiSelectionResponse ToApiResponse(this Selection selection)
    {
        return new ApiSelectionResponse(
            selection.Id,
            selection.Code.ToString(),
            selection.Name,
            selection.FairProbability.Value,
            selection.FairOdds.Value,
            selection.Odds.Value,
            selection.OddsVersion,
            selection.ExactScore.ToApiResponse());
    }

    public static ApiMarketResponse ToApiResponse(this Market market)
    {
        return new ApiMarketResponse(
            market.Id,
            market.Type.ToString(),
            market.Base?.Value,
            market.Margin,
            market.Selections.Select(selection => selection.ToApiResponse()).ToList());
    }

    public static ApiMatchSummaryResponse ToSummaryResponse(this Match match)
    {
        return new ApiMatchSummaryResponse(
            match.Id,
            match.Status.ToString(),
            match.PricingMode.ToString(),
            match.HomeTeamName,
            match.AwayTeamName,
            match.Competition,
            match.StartTime,
            match.Venue,
            match.LambdaHome,
            match.LambdaAway);
    }

    public static ApiMatchDetailsResponse ToDetailsResponse(this Match match)
    {
        return new ApiMatchDetailsResponse(
            match.Id,
            match.Status.ToString(),
            match.PricingMode.ToString(),
            match.HomeTeamName,
            match.AwayTeamName,
            match.Competition,
            match.StartTime,
            match.Venue,
            match.LambdaHome,
            match.LambdaAway,
            match.FinalScore.ToApiResponse(),
            match.Markets.Select(market => market.ToApiResponse()).ToList());
    }
}
