using TxConcert.Domain.Common;
using TxConcert.Domain.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class AiPromptConfiguration : IEntityTypeConfiguration<AiPromptEntity>
{
    public void Configure(EntityTypeBuilder<AiPromptEntity> builder)
    {
        builder.ToTable("ai_prompts");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.Key).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(Constants.Validation.MaxNameLength).IsRequired();
        builder.Property(e => e.SystemPrompt).HasMaxLength(Constants.Validation.MaxPromptLength).IsRequired();
        builder.Property(e => e.UserPromptInstructions).HasMaxLength(Constants.Validation.MaxPromptLength).IsRequired();
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(e => e.Key);
        builder.HasIndex(e => new { e.Key, e.VersionNumber }).IsUnique();
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.DeletedAt);
        builder.HasIndex(e => e.Key)
            .IsUnique()
            .HasDatabaseName("ix_ai_prompts_key_active")
            .HasFilter("\"IsActive\" = true");
    }
}
