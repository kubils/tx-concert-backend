using TxConcert.Domain.Features.Artists;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class ArtistGenreRepository(ApplicationDbContext context)
    : BaseRepository<ArtistGenreEntity>(context), IArtistGenreRepository
{
    public async Task<ArtistGenreEntity?> GetByArtistAndGenreAsync(
        string artistId, string genreId, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            e => e.ArtistId == artistId && e.GenreId == genreId, ct);
    }

    public async Task<IReadOnlyList<ArtistGenreEntity>> GetByArtistIdAsync(
        string artistId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.ArtistId == artistId)
            .ToListAsync(ct);
    }
}
