using SportsBook.Domain.Enums;

namespace SportsBook.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }

    private User()
    {
    }

    public User(
        Guid id,
        string email,
        string normalizedEmail,
        string passwordHash,
        UserRole role,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(normalizedEmail))
            throw new ArgumentException("Normalized email cannot be empty.", nameof(normalizedEmail));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        Id = id;
        Email = email.Trim();
        NormalizedEmail = normalizedEmail.Trim();
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public void MarkLoggedIn(DateTimeOffset now)
    {
        EnsureActive();

        LastLoginAt = now;
        UpdatedAt = now;
    }

    public void ChangePassword(
        string passwordHash,
        DateTimeOffset now)
    {
        EnsureActive();

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdatedAt = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = now;
    }

    public void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidOperationException("User is not active.");
    }
}
