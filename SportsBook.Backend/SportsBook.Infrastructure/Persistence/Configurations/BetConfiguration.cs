using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsBook.Domain.Entities;
using SportsBook.Infrastructure.Persistence.Converters;

namespace SportsBook.Infrastructure.Persistence.Configurations;

internal sealed class BetConfiguration : IEntityTypeConfiguration<Bet>
{
    public void Configure(EntityTypeBuilder<Bet> builder)
    {
        builder.ToTable("bets");

        builder.HasKey(bet => bet.Id);

        builder.Property(bet => bet.UserId)
            .IsRequired();

        builder.Property(bet => bet.MatchId)
            .IsRequired();

        builder.Property(bet => bet.MarketId)
            .IsRequired();

        builder.Property(bet => bet.SelectionId)
            .IsRequired();

        builder.Property(bet => bet.Stake)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(bet => bet.OddsSnapshot)
            .HasConversion(ValueObjectConverters.OddsConverter)
            .IsRequired();

        builder.Property(bet => bet.OddsVersionSnapshot)
            .IsRequired();

        builder.Property(bet => bet.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(bet => bet.PotentialPayout)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(bet => bet.ActualPayout)
            .HasPrecision(18, 2);

        builder.Property(bet => bet.CreatedAt)
            .IsRequired();

        builder.Property(bet => bet.SettledAt);

        builder.HasOne<Match>()
            .WithMany()
            .HasForeignKey(bet => bet.MatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Market>()
            .WithMany()
            .HasForeignKey(bet => bet.MarketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Selection>()
            .WithMany()
            .HasForeignKey(bet => bet.SelectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bet => bet.UserId);

        builder.HasIndex(bet => new
        {
            bet.UserId,
            bet.CreatedAt
        });

        builder.HasIndex(bet => bet.MatchId);

        builder.HasIndex(bet => bet.MarketId);

        builder.HasIndex(bet => bet.SelectionId);

        builder.HasIndex(bet => bet.Status);
    }
}