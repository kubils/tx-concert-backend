using TxConcert.Domain.Features.Venues;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class VenueRepository(ApplicationDbContext context)
    : BaseRepository<VenueEntity>(context), IVenueRepository
{
    public async Task<VenueEntity?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Slug == slug, ct);
    }

    public async Task<(IReadOnlyList<VenueEntity> Items, long Total)> GetByStateAsync(
        string stateId,
        int limit,
        int offset,
        CancellationToken ct = default)
    {
        IQueryable<VenueEntity> query = DbSet.Where(e => e.StateId == stateId);

        long total = await query.LongCountAsync(ct);

        IReadOnlyList<VenueEntity> items = await query
            .OrderBy(e => e.Name)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<VenueEntity> Items, long Total)> SearchAsync(
        string? searchTerm,
        string? stateId,
        int limit,
        int offset,
        CancellationToken ct = default)
    {
        IQueryable<VenueEntity> query = DbSet.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(e => EF.Functions.ILike(e.Name, $"%{searchTerm}%"));

        if (!string.IsNullOrEmpty(stateId))
            query = query.Where(e => e.StateId == stateId);

        long total = await query.LongCountAsync(ct);

        IReadOnlyList<VenueEntity> items = await query
            .OrderBy(e => e.Name)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }
}
