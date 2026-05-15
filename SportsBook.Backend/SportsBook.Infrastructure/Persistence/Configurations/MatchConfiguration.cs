using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsBook.Domain.Entities;
using SportsBook.Infrastructure.Persistence.Converters;

namespace SportsBook.Infrastructure.Persistence.Configurations;

internal sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");

        builder.HasKey(match => match.Id);

        builder.Property(match => match.HomeTeamName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(match => match.AwayTeamName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(match => match.Competition)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(match => match.Venue)
            .HasMaxLength(256);

        builder.Property(match => match.StartTime)
            .IsRequired();

        builder.Property(match => match.LambdaHome)
            .IsRequired();

        builder.Property(match => match.LambdaAway)
            .IsRequired();

        builder.Property(match => match.PricingMode)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(match => match.ModelVersion)
            .HasMaxLength(64);

        builder.Property(match => match.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(match => match.FinalScore)
            .HasConversion(ValueObjectConverters.NullableScoreConverter)
            .HasMaxLength(16);

        builder.Property(match => match.CreatedAt)
            .IsRequired();

        builder.Property(match => match.UpdatedAt);

        builder.HasMany<Market>("_markets")
            .WithOne()
            .HasForeignKey(market => market.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(match => match.Markets)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(match => match.StartTime);

        builder.HasIndex(match => new
        {
            match.Status,
            match.StartTime
        });

        builder.HasIndex(match => match.Competition);
    }
}
