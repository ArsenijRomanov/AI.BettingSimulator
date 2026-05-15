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

        builder.Navigation(wallet => wallet.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(wallet => wallet.UserId)
            .IsUnique();
    }
}