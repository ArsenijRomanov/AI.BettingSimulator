using Microsoft.EntityFrameworkCore;
using SportsBook.Application.Abstractions;
using SportsBook.Domain.Entities;

namespace SportsBook.Infrastructure.Persistence;

public sealed class SportsBookDbContext : DbContext, ISportsBookDbContext
{
    public SportsBookDbContext(DbContextOptions<SportsBookDbContext> options)
        : base(options)
    {
    }

    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Market> Markets => Set<Market>();
    public DbSet<Selection> Selections => Set<Selection>();

    public DbSet<Bet> Bets => Set<Bet>();

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<BalanceTransaction> BalanceTransactions => Set<BalanceTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SportsBookDbContext).Assembly);
    }
}