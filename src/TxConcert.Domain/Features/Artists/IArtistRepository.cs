using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Artists;

public interface IArtistRepository : IRepository<ArtistEntity>
{
    Task<ArtistEntity?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<(IReadOnlyList<ArtistEntity> Items, long Total)> SearchAsync(
        string? searchTerm,
        string? genreId,
        int limit,
        int offset,
        CancellationToken ct = default);

    Task<IReadOnlyList<ArtistEntity>> GetTopByPopularityAsync(
        int count,
        CancellationToken ct = default);
}
