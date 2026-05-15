using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Enums;

namespace SportsBook.Application.UseCases.Users;

public sealed record GetCurrentUserQuery(
    Guid UserId);

public sealed record CurrentUserResult(
    Guid UserId,
    string Email,
    UserRole Role,
    string? DisplayName,
    bool IsActive);

public sealed class GetCurrentUserHandler
{
    private readonly ISportsBookDbContext _dbContext;

    public GetCurrentUserHandler(ISportsBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurrentUserResult> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == query.UserId, cancellationToken);

        if (user is null)
            throw new InvalidOperationException("User was not found.");

        var displayName = user.Role == UserRole.Player
            ? await _dbContext.PlayerProfiles
                .Where(profile => profile.UserId == user.Id)
                .Select(profile => profile.DisplayName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        return new CurrentUserResult(
            user.Id,
            user.Email,
            user.Role,
            displayName,
            user.IsActive);
    }
}
