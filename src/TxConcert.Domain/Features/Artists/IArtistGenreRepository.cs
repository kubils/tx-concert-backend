using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Artists;

public interface IArtistGenreRepository : IRepository<ArtistGenreEntity>
{
    Task<ArtistGenreEntity?> GetByArtistAndGenreAsync(string artistId, string genreId, CancellationToken ct = default);
    Task<IReadOnlyList<ArtistGenreEntity>> GetByArtistIdAsync(string artistId, CancellationToken ct = default);
}
