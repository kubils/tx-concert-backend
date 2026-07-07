using NodaTime;
using TxConcert.Domain.Features.Genres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class GenreConfiguration : IEntityTypeConfiguration<GenreEntity>
{
    public void Configure(EntityTypeBuilder<GenreEntity> builder)
    {
        builder.ToTable("genres");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(100).IsRequired();

        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.Slug).IsUnique();

        SeedGenres(builder);
    }

    private static void SeedGenres(EntityTypeBuilder<GenreEntity> builder)
    {
        var now = Instant.FromUtc(2026, 4, 8, 0, 0, 0);

        var genres = new (string Name, string Slug, Guid Guid)[]
        {
            ("Country & Western",   "country-western",    new Guid("a1b2c3d4-0001-4000-8000-000000000001")),
            ("Pop & Rock",          "pop-rock",           new Guid("a1b2c3d4-0002-4000-8000-000000000002")),
            ("Hip-Hop & R&B",       "hip-hop-rnb",        new Guid("a1b2c3d4-0003-4000-8000-000000000003")),
            ("Latin & Tejano",      "latin-tejano",       new Guid("a1b2c3d4-0004-4000-8000-000000000004")),
            ("Electronic & Dance",  "electronic-dance",   new Guid("a1b2c3d4-0005-4000-8000-000000000005")),
            ("Jazz & Blues",        "jazz-blues",          new Guid("a1b2c3d4-0006-4000-8000-000000000006")),
            ("Classical & Opera",   "classical-opera",    new Guid("a1b2c3d4-0007-4000-8000-000000000007")),
        };

        builder.HasData(genres.Select(g => new
        {
            Id = $"genre_{g.Guid:N}",
            g.Name,
            g.Slug,
            CreatedAt = now,
            UpdatedAt = now,
            DeletedAt = (Instant?)null,
            CreatedById = (string?)null,
            CreatedByName = (string?)null,
            ModifiedById = (string?)null,
            ModifiedByName = (string?)null,
        }).ToArray());
    }
}
