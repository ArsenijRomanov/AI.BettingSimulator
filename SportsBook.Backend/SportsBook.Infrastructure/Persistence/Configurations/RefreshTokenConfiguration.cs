using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsBook.Domain.Entities;

namespace SportsBook.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.UserId)
            .IsRequired();

        builder.Property(token => token.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(token => token.CreatedAt)
            .IsRequired();

        builder.Property(token => token.ExpiresAt)
            .IsRequired();

        builder.Property(token => token.RevokedAt);

        builder.Property(token => token.ReplacedByTokenHash)
            .HasMaxLength(128);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(token => token.TokenHash)
            .IsUnique();

        builder.HasIndex(token => token.UserId);

        builder.HasIndex(token => token.ExpiresAt);
    }
}
