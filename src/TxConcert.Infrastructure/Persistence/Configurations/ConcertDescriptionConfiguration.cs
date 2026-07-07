using TxConcert.Domain.Features.Concerts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class ConcertDescriptionConfiguration : IEntityTypeConfiguration<ConcertDescriptionEntity>
{
    public void Configure(EntityTypeBuilder<ConcertDescriptionEntity> builder)
    {
        builder.ToTable("concert_descriptions");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.ConcertId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Content).HasMaxLength(10000).IsRequired();
        builder.Property(e => e.AiModel).HasMaxLength(100).IsRequired();
        builder.Property(e => e.PromptUsed).IsRequired();
        builder.Property(e => e.VersionNumber).IsRequired();
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.GeneratedAt).IsRequired();
        builder.Property(e => e.TokensUsed);
        builder.Property(e => e.GenerationTimeMs);

        builder.HasOne<ConcertEntity>()
            .WithMany()
            .HasForeignKey(e => e.ConcertId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ConcertId);
        builder.HasIndex(e => new { e.ConcertId, e.VersionNumber }).IsUnique();
        builder.HasIndex(e => new { e.ConcertId, e.IsActive });
    }
}
