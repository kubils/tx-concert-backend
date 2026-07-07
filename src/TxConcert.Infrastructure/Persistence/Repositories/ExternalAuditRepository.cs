using TxConcert.Domain.Features.ExternalAudit;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class ExternalAuditRepository(ApplicationDbContext context)
    : BaseRepository<ExternalAuditEntity>(context), IExternalAuditRepository
{
    public async Task<(IReadOnlyList<ExternalAuditEntity> Items, long Total)> GetPaginatedFilteredAsync(
        int limit,
        int offset,
        string? type = null,
        string? targetId = null,
        CancellationToken ct = default)
    {
        IQueryable<ExternalAuditEntity> query = DbSet.AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(e => e.Type == type);

        if (!string.IsNullOrEmpty(targetId))
            query = query.Where(e => e.TargetId == targetId);

        long total = await query.LongCountAsync(ct);

        IReadOnlyList<ExternalAuditEntity> items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<ExternalAuditEntity?> GetLatestByCorrelationIdAsync(
        string correlationId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.CorrelationId == correlationId && e.Latest)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ExternalAuditEntity>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.CorrelationId == correlationId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
    }
}
