using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SportsBook.Domain.Entities;

namespace SportsBook.Application.Abstractions;

public interface ISportsBookDbContext
{
    DbSet<Match> Matches { get; }
    DbSet<Market> Markets { get; }
    DbSet<Selection> Selections { get; }

    DbSet<Bet> Bets { get; }

    DbSet<Wallet> Wallets { get; }
    DbSet<BalanceTransaction> BalanceTransactions { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
