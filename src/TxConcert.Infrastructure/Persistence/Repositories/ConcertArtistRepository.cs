using TxConcert.Domain.Features.Concerts;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class ConcertArtistRepository(ApplicationDbContext context)
    : BaseRepository<ConcertArtistEntity>(context), IConcertArtistRepository
{
    public async Task<IReadOnlyList<ConcertArtistEntity>> GetByConcertIdAsync(
        string concertId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.ConcertId == concertId)
            .OrderBy(e => e.BillingOrder)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ConcertArtistEntity>> GetByArtistIdAsync(
        string artistId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.ArtistId == artistId)
            .ToListAsync(ct);
    }
}
