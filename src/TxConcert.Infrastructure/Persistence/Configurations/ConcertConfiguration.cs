using System.Text.Json;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class ConcertConfiguration : IEntityTypeConfiguration<ConcertEntity>
{
    public void Configure(EntityTypeBuilder<ConcertEntity> builder)
    {
        builder.ToTable("concerts");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(255).IsRequired();
        builder.Property(e => e.VenueId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.EventDate).IsRequired();
        builder.Property(e => e.DoorsOpen);
        builder.Property(e => e.StartTime);
        builder.Property(e => e.TimeZone).HasMaxLength(50).IsRequired().HasDefaultValue("America/Chicago");
        builder.Property(e => e.ConcertType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.TicketUrl).HasMaxLength(2048);
        builder.Property(e => e.PriceMin).HasPrecision(10, 2);
        builder.Property(e => e.PriceMax).HasPrecision(10, 2);
        builder.Property(e => e.IsSoldOut).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsFeatured).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.RescheduledFromId).HasMaxLength(128);
        builder.Property(e => e.TicketmasterId).HasMaxLength(128);
        builder.Property(e => e.DataHash).HasMaxLength(64);

        var externalDataConverter = new ValueConverter<ConcertExternalData?, string?>(
            externalData => SerializeExternalData(externalData),
            json => DeserializeExternalData(json));

        var externalDataComparer = new ValueComparer<ConcertExternalData?>(
            (l, r) => ExternalDataEquals(l, r),
            v => ExternalDataHashCode(v),
            v => ExternalDataSnapshot(v));

        builder.Property(e => e.ExternalData)
            .HasConversion(externalDataConverter)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb");

        builder.Property(e => e.ExternalData).Metadata.SetValueComparer(externalDataComparer);

        builder.HasOne<VenueEntity>()
            .WithMany()
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ConcertEntity>()
            .WithMany()
            .HasForeignKey(e => e.RescheduledFromId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.VenueId);
        builder.HasIndex(e => e.EventDate);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ConcertType);
        builder.HasIndex(e => e.IsFeatured);
        builder.HasIndex(e => e.TicketmasterId)
            .IsUnique()
            .HasFilter("\"TicketmasterId\" IS NOT NULL");
    }

    private static string? SerializeExternalData(ConcertExternalData? externalData)
        => externalData == null ? null : JsonSerializer.Serialize(externalData);

    private static ConcertExternalData? DeserializeExternalData(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<ConcertExternalData>(json);

    private static bool ExternalDataEquals(ConcertExternalData? left, ConcertExternalData? right)
        => JsonSerializer.Serialize(left) == JsonSerializer.Serialize(right);

    private static int ExternalDataHashCode(ConcertExternalData? value)
        => value == null ? 0 : JsonSerializer.Serialize(value).GetHashCode();

    private static ConcertExternalData? ExternalDataSnapshot(ConcertExternalData? value)
        => value == null ? null : JsonSerializer.Deserialize<ConcertExternalData>(JsonSerializer.Serialize(value));
}
