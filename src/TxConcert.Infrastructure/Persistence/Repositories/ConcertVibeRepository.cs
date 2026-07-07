using TxConcert.Domain.Features.Concerts;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class ConcertVibeRepository(ApplicationDbContext context)
    : BaseRepository<ConcertVibeEntity>(context), IConcertVibeRepository
{
    public async Task<ConcertVibeEntity?> GetByConcertIdAsync(string concertId, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.ConcertId == concertId, ct);
    }
}
