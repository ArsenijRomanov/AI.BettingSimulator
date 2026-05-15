namespace SportsBook.Application.Abstractions;

public sealed record MatchLambdaPredictionRequest(
    string HomeTeamName,
    string AwayTeamName,
    string Competition,
    DateTimeOffset StartTime);

public sealed record MatchLambdaPredictionResult(
    double LambdaHome,
    double LambdaAway,
    string ModelVersion,
    bool IsStub);

public interface IMatchPredictionClient
{
    Task<MatchLambdaPredictionResult> PredictLambdasAsync(
        MatchLambdaPredictionRequest request,
        CancellationToken cancellationToken = default);
}
