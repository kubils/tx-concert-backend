using TxConcert.Domain.Features.Artists;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class ArtistVibeProfileRepository(ApplicationDbContext context)
    : BaseRepository<ArtistVibeProfileEntity>(context), IArtistVibeProfileRepository
{
    public async Task<ArtistVibeProfileEntity?> GetByArtistIdAsync(string artistId, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.ArtistId == artistId, ct);
    }
}
