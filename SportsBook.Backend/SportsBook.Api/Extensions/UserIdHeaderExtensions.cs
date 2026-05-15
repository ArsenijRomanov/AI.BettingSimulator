using System.Security.Claims;

namespace SportsBook.Api.Extensions;

public static class UserIdHeaderExtensions
{
    public static Guid GetRequiredUserId(this HttpContext context)
    {
        var rawValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? context.User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(rawValue))
            throw new InvalidOperationException("Authenticated user id was not found.");

        if (!Guid.TryParse(rawValue, out var userId))
            throw new InvalidOperationException("Authenticated user id is invalid.");

        return userId;
    }
}
