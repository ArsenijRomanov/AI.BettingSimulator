using SportsBook.Domain.Entities;

namespace SportsBook.Application.Abstractions;

public interface IAuthLockService
{
    Task<RefreshToken?> LockRefreshTokenByHashForUpdateAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);
}