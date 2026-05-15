using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Entities;
using SportsBook.Domain.Enums;

namespace SportsBook.Application.UseCases.Auth;

public sealed record RegisterOperatorCommand(
    string Email,
    string Password);

public sealed class RegisterOperatorHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenService _authTokenService;

    public RegisterOperatorHandler(
        ISportsBookDbContext dbContext,
        IClock clock,
        IPasswordHasher passwordHasher,
        IAuthTokenService authTokenService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _passwordHasher = passwordHasher;
        _authTokenService = authTokenService;
    }

    public async Task<AuthResult> Handle(
        RegisterOperatorCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
            throw new ArgumentException("Password must contain at least 6 characters.", nameof(command.Password));

        var now = _clock.UtcNow;
        var normalizedEmail = EmailNormalizer.Normalize(command.Email);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var userExists = await _dbContext.Users
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (userExists)
            throw new InvalidOperationException("User with this email already exists.");

        var user = new User(
            id: Guid.NewGuid(),
            email: command.Email,
            normalizedEmail: normalizedEmail,
            passwordHash: _passwordHasher.Hash(command.Password),
            role: UserRole.Operator,
            createdAt: now);

        _dbContext.Users.Add(user);

        var accessToken = _authTokenService.CreateAccessToken(user);
        var refreshToken = _authTokenService.CreateRefreshToken();
        var refreshTokenHash = _authTokenService.HashRefreshToken(refreshToken);
        var refreshTokenExpiresAt = _authTokenService.GetRefreshTokenExpiresAt(now);

        _dbContext.RefreshTokens.Add(new RefreshToken(
            id: Guid.NewGuid(),
            userId: user.Id,
            tokenHash: refreshTokenHash,
            createdAt: now,
            expiresAt: refreshTokenExpiresAt));

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AuthResult(
            user.Id,
            user.Email,
            user.Role,
            DisplayName: null,
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken,
            refreshTokenExpiresAt);
    }
}
