using TxConcert.Domain.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class CommandOutboxConfiguration : IEntityTypeConfiguration<CommandOutboxEntity>
{
    public void Configure(EntityTypeBuilder<CommandOutboxEntity> builder)
    {
        builder.ToTable("command_outbox");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.JobId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Command).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Content).IsRequired();
        builder.Property(e => e.OccurredOn).IsRequired();
        builder.Property(e => e.PublishedOn);
        builder.Property(e => e.ProcessedOn);
        builder.Property(e => e.IsSuccessful);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.Property(e => e.IsRetry).HasDefaultValue(false);

        builder.HasIndex(e => e.PublishedOn);
        builder.HasIndex(e => e.ProcessedOn);
        builder.HasIndex(e => e.Command);
    }
}
