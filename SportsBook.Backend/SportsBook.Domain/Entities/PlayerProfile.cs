namespace SportsBook.Domain.Entities;

public sealed class PlayerProfile
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private PlayerProfile()
    {
    }

    public PlayerProfile(
        Guid id,
        Guid userId,
        string displayName,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Player profile id cannot be empty.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        Id = id;
        UserId = userId;
        DisplayName = displayName.Trim();
        CreatedAt = createdAt;
    }

    public void Update(
        string displayName,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        DisplayName = displayName.Trim();
        UpdatedAt = now;
    }
}
