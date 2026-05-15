using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;

namespace SportsBook.Application.UseCases.Wallets;

public sealed record GetWalletQuery(Guid UserId);

public sealed record GetWalletResult(
    Guid UserId,
    decimal Balance);

public sealed class GetWalletHandler
{
    private readonly ISportsBookDbContext _dbContext;

    public GetWalletHandler(ISportsBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetWalletResult> Handle(
        GetWalletQuery query,
        CancellationToken cancellationToken = default)
    {
        var wallet = await _dbContext.Wallets
            .FirstOrDefaultAsync(wallet => wallet.UserId == query.UserId, cancellationToken);

        if (wallet is null)
            throw new InvalidOperationException("Wallet was not found.");

        return new GetWalletResult(
            wallet.UserId,
            wallet.Balance);
    }
}