using TxConcert.Domain.Features.Concerts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class ConcertArticleConfiguration : IEntityTypeConfiguration<ConcertArticleEntity>
{
    public void Configure(EntityTypeBuilder<ConcertArticleEntity> builder)
    {
        builder.ToTable("concert_articles");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.ConcertId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Spot).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.Body).IsRequired();
        builder.Property(e => e.SeoKeywords).HasColumnType("text[]").IsRequired();
        builder.Property(e => e.MetaDescription).HasMaxLength(500).IsRequired();
        builder.Property(e => e.ImageUrl).HasMaxLength(2048);
        builder.Property(e => e.ImageAltText).HasMaxLength(500);
        builder.Property(e => e.ImageCredit).HasMaxLength(255);
        builder.Property(e => e.ImageSource)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
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

        // Exactly one active article per concert.
        builder.HasIndex(e => e.ConcertId)
            .IsUnique()
            .HasDatabaseName("ix_concert_articles_concert_id_active")
            .HasFilter("\"IsActive\" = true");
    }
}
