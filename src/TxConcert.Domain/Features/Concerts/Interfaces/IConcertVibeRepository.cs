using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Concerts;

public interface IConcertVibeRepository : IRepository<ConcertVibeEntity>
{
    Task<ConcertVibeEntity?> GetByConcertIdAsync(string concertId, CancellationToken ct = default);
}
