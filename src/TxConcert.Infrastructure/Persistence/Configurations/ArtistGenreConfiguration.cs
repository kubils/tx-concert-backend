using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Genres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class ArtistGenreConfiguration : IEntityTypeConfiguration<ArtistGenreEntity>
{
    public void Configure(EntityTypeBuilder<ArtistGenreEntity> builder)
    {
        builder.ToTable("artist_genres");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.ArtistId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.GenreId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.IsPrimary).IsRequired().HasDefaultValue(false);

        builder.HasOne<ArtistEntity>()
            .WithMany()
            .HasForeignKey(e => e.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<GenreEntity>()
            .WithMany()
            .HasForeignKey(e => e.GenreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ArtistId, e.GenreId }).IsUnique();
        builder.HasIndex(e => e.GenreId);
    }
}
