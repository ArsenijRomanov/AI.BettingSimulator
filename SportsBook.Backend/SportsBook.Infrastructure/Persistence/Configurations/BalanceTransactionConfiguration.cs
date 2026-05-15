using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsBook.Domain.Entities;

namespace SportsBook.Infrastructure.Persistence.Configurations;

internal sealed class BalanceTransactionConfiguration : IEntityTypeConfiguration<BalanceTransaction>
{
    public void Configure(EntityTypeBuilder<BalanceTransaction> builder)
    {
        builder.ToTable("balance_transactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.WalletId)
            .IsRequired();

        builder.Property(transaction => transaction.UserId)
            .IsRequired();

        builder.Property(transaction => transaction.BetId);

        builder.Property(transaction => transaction.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(transaction => transaction.BalanceAfter)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(transaction => transaction.CreatedAt)
            .IsRequired();
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(transaction => transaction.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Bet>()
            .WithMany()
            .HasForeignKey(transaction => transaction.BetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transaction => transaction.WalletId);

        builder.HasIndex(transaction => transaction.UserId);

        builder.HasIndex(transaction => transaction.BetId);

        builder.HasIndex(transaction => new
        {
            transaction.UserId,
            transaction.CreatedAt
        });
    }
}
