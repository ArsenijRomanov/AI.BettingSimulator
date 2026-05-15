using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsBook.Domain.Entities;

namespace SportsBook.Infrastructure.Persistence.Configurations;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");

        builder.HasKey(wallet => wallet.Id);

        builder.Property(wallet => wallet.UserId)
            .IsRequired();

        builder.Property(wallet => wallet.Balance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(wallet => wallet.CreatedAt)
            .IsRequired();

        builder.Property(wallet => wallet.UpdatedAt);

        builder.HasMany(wallet => wallet.Transactions)
            .WithOne()
            .HasForeignKey(transaction => transaction.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(wallet => wallet.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(wallet => wallet.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wallet => wallet.UserId)
            .IsUnique();
    }
}
