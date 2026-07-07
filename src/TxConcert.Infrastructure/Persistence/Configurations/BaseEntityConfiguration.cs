using TxConcert.Domain.Common.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public static class BaseEntityConfiguration
{
    public static void ApplyBaseEntityConfiguration<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(128).IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.DeletedAt);
        builder.Property(e => e.CreatedById).HasMaxLength(255);
        builder.Property(e => e.ModifiedById).HasMaxLength(255);
        builder.Property(e => e.CreatedByName).HasMaxLength(255);
        builder.Property(e => e.ModifiedByName).HasMaxLength(255);

        builder.HasIndex(e => e.DeletedAt);
    }
}
