using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsBook.Domain.Entities;
using SportsBook.Infrastructure.Persistence.Converters;

namespace SportsBook.Infrastructure.Persistence.Configurations;

internal sealed class SelectionConfiguration : IEntityTypeConfiguration<Selection>
{
    public void Configure(EntityTypeBuilder<Selection> builder)
    {
        builder.ToTable("selections");

        builder.HasKey(selection => selection.Id);

        builder.Property(selection => selection.MarketId)
            .IsRequired();

        builder.Property(selection => selection.Code)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(selection => selection.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(selection => selection.FairProbability)
            .HasConversion(ValueObjectConverters.ProbabilityConverter)
            .IsRequired();

        builder.Property(selection => selection.FairOdds)
            .HasConversion(ValueObjectConverters.OddsConverter)
            .IsRequired();

        builder.Property(selection => selection.Odds)
            .HasConversion(ValueObjectConverters.OddsConverter)
            .IsRequired();

        builder.Property(selection => selection.OddsVersion)
            .IsRequired();

        builder.Property(selection => selection.IsActive)
            .IsRequired();

        builder.Property(selection => selection.ExactScore)
            .HasConversion(ValueObjectConverters.NullableScoreConverter)
            .HasMaxLength(16);

        builder.HasIndex(selection => selection.MarketId);

        // Regular selections:
        // Home/Draw/Away, Over/Under.
        // CorrectScore selections are excluded because all of them have Code = ExactScore.
        builder.HasIndex(selection => new
        {
            selection.MarketId,
            selection.Code
        })
        .IsUnique()
        .HasFilter("\"ExactScore\" IS NULL");

        // CorrectScore selections:
        // one exact score can appear only once inside one market.
        builder.HasIndex(selection => new
        {
            selection.MarketId,
            selection.ExactScore
        })
        .IsUnique()
        .HasFilter("\"ExactScore\" IS NOT NULL");
    }
}
