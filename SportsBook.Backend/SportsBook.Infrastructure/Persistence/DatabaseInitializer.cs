using Microsoft.EntityFrameworkCore;

namespace SportsBook.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly SportsBookDbContext _dbContext;

    public DatabaseInitializer(SportsBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
