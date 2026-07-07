using TxConcert.Domain.Features.ExternalAudit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class ExternalAuditConfiguration : IEntityTypeConfiguration<ExternalAuditEntity>
{
    public void Configure(EntityTypeBuilder<ExternalAuditEntity> builder)
    {
        builder.ToTable("external_audits");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.CorrelationId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Type).HasMaxLength(100).IsRequired();
        builder.Property(e => e.RequestUrl).HasMaxLength(2048).IsRequired();
        builder.Property(e => e.RequestBody).IsRequired();
        builder.Property(e => e.ResponseBody);
        builder.Property(e => e.StatusCode);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.Property(e => e.CanRetry).HasDefaultValue(false);
        builder.Property(e => e.TargetId).HasMaxLength(128);
        builder.Property(e => e.Attempts).HasDefaultValue(1);
        builder.Property(e => e.Latest).HasDefaultValue(false);
        builder.Property(e => e.ResponseTime).HasDefaultValue(0L);

        builder.HasIndex(e => e.CorrelationId);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.TargetId);
        builder.HasIndex(e => e.Latest);
    }
}
