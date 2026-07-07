using TxConcert.Domain.Features.Concerts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class ConcertVibeConfiguration : IEntityTypeConfiguration<ConcertVibeEntity>
{
    public void Configure(EntityTypeBuilder<ConcertVibeEntity> builder)
    {
        builder.ToTable("concert_vibes");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.ConcertId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Energy).IsRequired();
        builder.Property(e => e.SoundQuality).IsRequired();
        builder.Property(e => e.Hype).IsRequired();
        builder.Property(e => e.CrowdVibe).IsRequired();
        builder.Property(e => e.ValueForMoney).IsRequired();
        builder.Property(e => e.AiSummary).HasMaxLength(5000);
        builder.Property(e => e.AiModel).HasMaxLength(100);

        builder.HasOne<ConcertEntity>()
            .WithOne()
            .HasForeignKey<ConcertVibeEntity>(e => e.ConcertId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ConcertId).IsUnique();
    }
}
