using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsBook.Domain.Entities;
using SportsBook.Infrastructure.Persistence.Converters;

namespace SportsBook.Infrastructure.Persistence.Configurations;

internal sealed class MarketConfiguration : IEntityTypeConfiguration<Market>
{
    public void Configure(EntityTypeBuilder<Market> builder)
    {
        builder.ToTable("markets");

        builder.HasKey(market => market.Id);

        builder.Property(market => market.MatchId)
            .IsRequired();

        builder.Property(market => market.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(market => market.Base)
            .HasConversion(ValueObjectConverters.NullableMarketBaseConverter);

        builder.Property(market => market.Margin)
            .IsRequired();

        builder.Property(market => market.IsActive)
            .IsRequired();

        builder.HasMany(market => market.Selections)
            .WithOne()
            .HasForeignKey(selection => selection.MarketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(market => market.Selections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(market => market.MatchId);

        // Markets with base:
        // Total 2.5, Handicap -1.5, HomeTotal 1.5, etc.
        builder.HasIndex(market => new
            {
                market.MatchId,
                market.Type,
                market.Base
            })
            .IsUnique()
            .HasFilter("\"Base\" IS NOT NULL");

        // Markets without base:
        // HomeDrawAway, CorrectScore.
        // PostgreSQL allows many NULL values in a unique index,
        // so this separate filtered index is needed.
        builder.HasIndex(market => new
            {
                market.MatchId,
                market.Type
            })
            .IsUnique()
            .HasFilter("\"Base\" IS NULL");
    }
}
