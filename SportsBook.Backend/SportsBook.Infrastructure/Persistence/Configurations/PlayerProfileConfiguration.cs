using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsBook.Domain.Entities;

namespace SportsBook.Infrastructure.Persistence.Configurations;

internal sealed class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.ToTable("player_profiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.UserId)
            .IsRequired();

        builder.Property(profile => profile.DisplayName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(profile => profile.CreatedAt)
            .IsRequired();

        builder.Property(profile => profile.UpdatedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(profile => profile.UserId)
            .IsUnique();
    }
}
