using TxConcert.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class EventOutboxConfiguration : IEntityTypeConfiguration<EventOutboxEntity>
{
    public void Configure(EntityTypeBuilder<EventOutboxEntity> builder)
    {
        builder.ToTable("event_outbox");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.JobId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.EventName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Content).IsRequired();
        builder.Property(e => e.OccurredOn).IsRequired();
        builder.Property(e => e.PublishedOn);
        builder.Property(e => e.ProcessedOn);
        builder.Property(e => e.IsSuccessful);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);

        builder.Property(e => e.ProcessedBy)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        builder.HasIndex(e => e.PublishedOn);
        builder.HasIndex(e => e.EventName);
    }
}
