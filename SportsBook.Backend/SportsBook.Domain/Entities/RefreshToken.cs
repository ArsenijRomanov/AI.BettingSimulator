namespace SportsBook.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    private RefreshToken()
    {
    }

    public RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Refresh token id cannot be empty.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash cannot be empty.", nameof(tokenHash));

        if (expiresAt <= createdAt)
            throw new ArgumentException("Refresh token expiration must be after creation time.", nameof(expiresAt));

        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public bool IsActive(DateTimeOffset now)
    {
        return RevokedAt is null && ExpiresAt > now;
    }

    public void Revoke(
        DateTimeOffset now,
        string? replacedByTokenHash = null)
    {
        if (RevokedAt is not null)
            return;

        RevokedAt = now;
        ReplacedByTokenHash = string.IsNullOrWhiteSpace(replacedByTokenHash)
            ? null
            : replacedByTokenHash;
    }
}
