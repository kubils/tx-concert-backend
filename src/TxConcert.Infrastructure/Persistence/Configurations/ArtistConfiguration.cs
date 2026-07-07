using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class ArtistConfiguration : IEntityTypeConfiguration<ArtistEntity>
{
    public void Configure(EntityTypeBuilder<ArtistEntity> builder)
    {
        builder.ToTable("artists");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Bio).HasMaxLength(5000);
        builder.Property(e => e.ImageUrl).HasMaxLength(2048);
        builder.Property(e => e.OriginCity).HasMaxLength(255);
        builder.Property(e => e.OriginStateId).HasMaxLength(128);
        builder.Property(e => e.OriginCountry).HasMaxLength(100).IsRequired().HasDefaultValue("US");
        builder.Property(e => e.PopularityScore).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.SpotifyUrl).HasMaxLength(2048);
        builder.Property(e => e.InstagramUrl).HasMaxLength(2048);
        builder.Property(e => e.WebsiteUrl).HasMaxLength(2048);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasOne<StateEntity>()
            .WithMany()
            .HasForeignKey(e => e.OriginStateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.OriginStateId);
        builder.HasIndex(e => e.IsActive);
    }
}
