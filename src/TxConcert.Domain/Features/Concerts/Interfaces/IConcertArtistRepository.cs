using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Concerts;

public interface IConcertArtistRepository : IRepository<ConcertArtistEntity>
{
    Task<IReadOnlyList<ConcertArtistEntity>> GetByConcertIdAsync(string concertId, CancellationToken ct = default);
    Task<IReadOnlyList<ConcertArtistEntity>> GetByArtistIdAsync(string artistId, CancellationToken ct = default);
}
