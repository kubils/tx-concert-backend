using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Concerts;

public interface IConcertArticleRepository : IRepository<ConcertArticleEntity>
{
    Task<ConcertArticleEntity?> GetActiveForConcertAsync(
        string concertId,
        CancellationToken ct = default);

    Task<IReadOnlyList<ConcertArticleEntity>> GetAllForConcertAsync(
        string concertId,
        CancellationToken ct = default);

    Task<int> GetNextVersionNumberAsync(
        string concertId,
        CancellationToken ct = default);
}
