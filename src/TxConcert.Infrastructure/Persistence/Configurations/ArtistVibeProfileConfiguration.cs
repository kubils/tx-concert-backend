using TxConcert.Domain.Features.Artists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class ArtistVibeProfileConfiguration : IEntityTypeConfiguration<ArtistVibeProfileEntity>
{
    public void Configure(EntityTypeBuilder<ArtistVibeProfileEntity> builder)
    {
        builder.ToTable("artist_vibe_profiles");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.ArtistId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Visuals).IsRequired();
        builder.Property(e => e.Sound).IsRequired();
        builder.Property(e => e.Energy).IsRequired();
        builder.Property(e => e.FanInteraction).IsRequired();
        builder.Property(e => e.TexasSpirit).IsRequired();
        builder.Property(e => e.AiSummary).HasMaxLength(5000);
        builder.Property(e => e.AiModel).HasMaxLength(100);

        builder.HasOne<ArtistEntity>()
            .WithOne()
            .HasForeignKey<ArtistVibeProfileEntity>(e => e.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ArtistId).IsUnique();
    }
}
