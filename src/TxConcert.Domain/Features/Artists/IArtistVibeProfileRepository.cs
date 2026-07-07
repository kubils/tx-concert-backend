using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Artists;

public interface IArtistVibeProfileRepository : IRepository<ArtistVibeProfileEntity>
{
    Task<ArtistVibeProfileEntity?> GetByArtistIdAsync(string artistId, CancellationToken ct = default);
}
