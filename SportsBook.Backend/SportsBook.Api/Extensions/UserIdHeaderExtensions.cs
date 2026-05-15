namespace SportsBook.Api.Extensions;

public static class UserIdHeaderExtensions
{
    private const string HeaderName = "X-User-Id";

    public static Guid GetRequiredUserId(this HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var values))
            throw new InvalidOperationException($"Header '{HeaderName}' is required.");

        var rawValue = values.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(rawValue))
            throw new InvalidOperationException($"Header '{HeaderName}' cannot be empty.");

        if (!Guid.TryParse(rawValue, out var userId))
            throw new InvalidOperationException($"Header '{HeaderName}' must be a valid GUID.");

        return userId;
    }
}
