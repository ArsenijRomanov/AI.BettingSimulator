using System.Net.Http.Json;
using SportsBook.Application.Abstractions;

namespace SportsBook.Infrastructure.ML;

public sealed class HttpMatchPredictionClient : IMatchPredictionClient
{
    private readonly HttpClient _httpClient;

    public HttpMatchPredictionClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MatchLambdaPredictionResult> PredictLambdasAsync(
        MatchLambdaPredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var mlRequest = new MlLambdaPredictionRequest(
            request.HomeTeamName,
            request.AwayTeamName,
            request.Competition,
            request.StartTime);

        using var response = await _httpClient.PostAsJsonAsync(
            "/predict-lambdas",
            mlRequest,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new InvalidOperationException(
                $"ML service returned {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
        }

        var prediction = await response.Content.ReadFromJsonAsync<MlLambdaPredictionResponse>(
            cancellationToken: cancellationToken);

        if (prediction is null)
            throw new InvalidOperationException("ML service returned empty prediction response.");

        if (!double.IsFinite(prediction.LambdaHome) || prediction.LambdaHome <= 0)
            throw new InvalidOperationException("ML service returned invalid lambdaHome.");

        if (!double.IsFinite(prediction.LambdaAway) || prediction.LambdaAway <= 0)
            throw new InvalidOperationException("ML service returned invalid lambdaAway.");

        if (string.IsNullOrWhiteSpace(prediction.ModelVersion))
            throw new InvalidOperationException("ML service returned empty modelVersion.");

        return new MatchLambdaPredictionResult(
            prediction.LambdaHome,
            prediction.LambdaAway,
            prediction.ModelVersion,
            prediction.IsStub);
    }

    private sealed record MlLambdaPredictionRequest(
        string HomeTeamName,
        string AwayTeamName,
        string Competition,
        DateTimeOffset StartTime);

    private sealed record MlLambdaPredictionResponse(
        double LambdaHome,
        double LambdaAway,
        string ModelVersion,
        bool IsStub);
}
