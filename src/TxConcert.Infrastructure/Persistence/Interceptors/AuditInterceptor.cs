using TxConcert.Application.Common.Interfaces;
using TxConcert.Domain.Common.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime;

namespace TxConcert.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Sets CreatedAt/UpdatedAt and audit fields automatically on SaveChanges.
/// NestJS equivalent: TypeORM's @BeforeInsert/@BeforeUpdate or subscriber hooks.
/// </summary>
public sealed class AuditInterceptor(IClock clock, ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, ct);

        Instant now = clock.GetCurrentInstant();

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry in
            eventData.Context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreatedAt(now);
                    entry.Entity.SetUpdatedAt(now);
                    if (currentUser.IsAuthenticated)
                        entry.Entity.SetAuditFields(currentUser.UserId!, currentUser.DisplayName);
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdatedAt(now);
                    if (currentUser.IsAuthenticated)
                        entry.Entity.SetAuditFields(currentUser.UserId!, currentUser.DisplayName);
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}
