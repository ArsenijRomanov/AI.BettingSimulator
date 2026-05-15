using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;

namespace SportsBook.Application.UseCases.Users;

public sealed record UpdatePlayerProfileCommand(
    Guid UserId,
    string DisplayName);

public sealed record UpdatePlayerProfileResult(
    Guid UserId,
    string DisplayName);

public sealed class UpdatePlayerProfileHandler
{
    private readonly ISportsBookDbContext _dbContext;
    private readonly IClock _clock;

    public UpdatePlayerProfileHandler(
        ISportsBookDbContext dbContext,
        IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<UpdatePlayerProfileResult> Handle(
        UpdatePlayerProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;

        var profile = await _dbContext.PlayerProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == command.UserId, cancellationToken);

        if (profile is null)
            throw new InvalidOperationException("Player profile was not found.");

        profile.Update(command.DisplayName, now);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdatePlayerProfileResult(
            profile.UserId,
            profile.DisplayName);
    }
}