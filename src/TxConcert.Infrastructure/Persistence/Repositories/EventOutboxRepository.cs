using TxConcert.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class EventOutboxRepository(ApplicationDbContext context)
    : BaseRepository<EventOutboxEntity>(context)
{
    public async Task<IReadOnlyList<EventOutboxEntity>> GetUnpublishedAsync(
        int batchSize = 50,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.PublishedOn == null && e.DeletedAt == null)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<int> CleanupSuccessfulAsync(
        NodaTime.Instant olderThan,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.IsSuccessful == true && e.ProcessedOn != null && e.ProcessedOn < olderThan)
            .ExecuteDeleteAsync(ct);
    }
}
