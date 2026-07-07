using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Concerts;

public interface IConcertDescriptionRepository : IRepository<ConcertDescriptionEntity>
{
    Task<ConcertDescriptionEntity?> GetActiveForConcertAsync(
        string concertId,
        CancellationToken ct = default);

    Task<IReadOnlyList<ConcertDescriptionEntity>> GetAllForConcertAsync(
        string concertId,
        CancellationToken ct = default);

    Task<int> GetNextVersionNumberAsync(
        string concertId,
        CancellationToken ct = default);
}
