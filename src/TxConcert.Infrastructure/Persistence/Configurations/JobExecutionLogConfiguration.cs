using TxConcert.Domain.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class JobExecutionLogConfiguration : IEntityTypeConfiguration<JobExecutionLogEntity>
{
    public void Configure(EntityTypeBuilder<JobExecutionLogEntity> builder)
    {
        builder.ToTable("job_execution_logs");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.JobName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.JobGroup).HasMaxLength(255).IsRequired();
        builder.Property(e => e.TriggerName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.TriggerGroup).HasMaxLength(255).IsRequired();
        builder.Property(e => e.FireInstanceId).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.StartedAt).IsRequired();
        builder.Property(e => e.CompletedAt);
        builder.Property(e => e.DurationMs);
        builder.Property(e => e.ErrorMessage).HasMaxLength(4000);

        builder.HasIndex(e => e.FireInstanceId).IsUnique();
        builder.HasIndex(e => e.JobName);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.StartedAt);
    }
}
