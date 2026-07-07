using TxConcert.Domain.Features.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TxConcert.Infrastructure.Persistence.Configurations;

public sealed class StateConfiguration : IEntityTypeConfiguration<StateEntity>
{
    public void Configure(EntityTypeBuilder<StateEntity> builder)
    {
        builder.ToTable("states");
        builder.ApplyBaseEntityConfiguration();

        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Abbreviation).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Country).HasMaxLength(100).IsRequired().HasDefaultValue("US");
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.SortOrder).IsRequired().HasDefaultValue(0);

        builder.HasIndex(e => e.Abbreviation).IsUnique();
        builder.HasIndex(e => e.IsActive);
    }
}
