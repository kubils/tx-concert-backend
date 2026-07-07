using TxConcert.Domain.Features.States;
using TxConcert.Domain.Features.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class VenueConfiguration : IEntityTypeConfiguration<VenueEntity>
{
    public void Configure(EntityTypeBuilder<VenueEntity> builder)
    {
        builder.ToTable("venues");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Address).HasMaxLength(500).IsRequired();
        builder.Property(e => e.City).HasMaxLength(255).IsRequired();
        builder.Property(e => e.StateId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Country).HasMaxLength(100).IsRequired().HasDefaultValue("US");
        builder.Property(e => e.ZipCode).HasMaxLength(20);
        builder.Property(e => e.Capacity);
        builder.Property(e => e.Latitude).HasPrecision(12, 7);
        builder.Property(e => e.Longitude).HasPrecision(12, 7);
        builder.Property(e => e.VenueType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.WebsiteUrl).HasMaxLength(2048);
        builder.Property(e => e.PhoneNumber).HasMaxLength(50);

        builder.HasOne<StateEntity>()
            .WithMany()
            .HasForeignKey(e => e.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.StateId);
        builder.HasIndex(e => new { e.City, e.StateId });
    }
}
