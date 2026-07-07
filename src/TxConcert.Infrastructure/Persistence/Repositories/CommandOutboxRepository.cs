using TxConcert.Domain.Commands;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class CommandOutboxRepository(ApplicationDbContext context)
    : BaseRepository<CommandOutboxEntity>(context)
{
    public async Task<IReadOnlyList<CommandOutboxEntity>> GetUnpublishedAsync(
        int batchSize = 50,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.PublishedOn == null && c.DeletedAt == null)
            .OrderBy(c => c.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CommandOutboxEntity>> GetStaleProcessingAsync(
        NodaTime.Instant threshold,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.PublishedOn != null && c.ProcessedOn == null && c.CreatedAt < threshold)
            .ToListAsync(ct);
    }

    public async Task<int> CleanupSuccessfulAsync(
        NodaTime.Instant olderThan,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.IsSuccessful == true && c.ProcessedOn != null && c.ProcessedOn < olderThan)
            .ExecuteDeleteAsync(ct);
    }
}
