namespace SportsBook.Application.UseCases.Auth;

internal static class EmailNormalizer
{
    public static string Normalize(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        var normalized = email.Trim().ToUpperInvariant();

        if (!normalized.Contains('@'))
            throw new ArgumentException("Email is invalid.", nameof(email));

        return normalized;
    }
}
