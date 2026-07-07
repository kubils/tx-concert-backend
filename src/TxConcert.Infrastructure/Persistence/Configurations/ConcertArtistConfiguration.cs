using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class ConcertArtistConfiguration : IEntityTypeConfiguration<ConcertArtistEntity>
{
    public void Configure(EntityTypeBuilder<ConcertArtistEntity> builder)
    {
        builder.ToTable("concert_artists");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.ConcertId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.ArtistId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.IsHeadliner).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.BillingOrder).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.SetTime);

        builder.HasOne<ConcertEntity>()
            .WithMany()
            .HasForeignKey(e => e.ConcertId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ArtistEntity>()
            .WithMany()
            .HasForeignKey(e => e.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ConcertId, e.ArtistId }).IsUnique();
        builder.HasIndex(e => e.ArtistId);
        builder.HasIndex(e => new { e.ConcertId, e.BillingOrder });
    }
}
