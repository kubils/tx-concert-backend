using TxConcert.Domain.Features.Concerts;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class ConcertArticleRepository(ApplicationDbContext context)
    : BaseRepository<ConcertArticleEntity>(context), IConcertArticleRepository
{
    public async Task<ConcertArticleEntity?> GetActiveForConcertAsync(
        string concertId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.ConcertId == concertId && e.IsActive)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ConcertArticleEntity>> GetAllForConcertAsync(
        string concertId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.ConcertId == concertId)
            .OrderByDescending(e => e.VersionNumber)
            .ToListAsync(ct);
    }

    public async Task<int> GetNextVersionNumberAsync(
        string concertId,
        CancellationToken ct = default)
    {
        int maxVersion = await DbSet
            .Where(e => e.ConcertId == concertId)
            .MaxAsync(e => (int?)e.VersionNumber, ct) ?? 0;

        return maxVersion + 1;
    }
}
