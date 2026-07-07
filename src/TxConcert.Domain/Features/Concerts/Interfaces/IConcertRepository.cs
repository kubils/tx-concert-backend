using TxConcert.Domain.Common.Base;
using TxConcert.Domain.Common.Enums;
using NodaTime;

namespace TxConcert.Domain.Features.Concerts;

public interface IConcertRepository : IRepository<ConcertEntity>
{
    Task<ConcertEntity?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<ConcertEntity?> GetByTicketmasterIdAsync(string ticketmasterId, CancellationToken ct = default);

    Task<(IReadOnlyList<ConcertEntity> Items, long Total)> GetUpcomingAsync(
        int limit,
        int offset,
        CancellationToken ct = default);

    Task<(IReadOnlyList<ConcertEntity> Items, long Total)> GetByStateAsync(
        string stateId,
        int limit,
        int offset,
        CancellationToken ct = default);

    Task<(IReadOnlyList<ConcertEntity> Items, long Total)> GetByArtistAsync(
        string artistId,
        int limit,
        int offset,
        CancellationToken ct = default);

    Task<(IReadOnlyList<ConcertEntity> Items, long Total)> GetByDateRangeAsync(
        LocalDate from,
        LocalDate to,
        int limit,
        int offset,
        CancellationToken ct = default);

    Task<(IReadOnlyList<ConcertEntity> Items, long Total)> SearchAsync(
        string? searchTerm,
        string? stateId,
        string? artistId,
        ConcertType? concertType,
        LocalDate? fromDate,
        LocalDate? toDate,
        int limit,
        int offset,
        CancellationToken ct = default);

    Task<IReadOnlyList<ConcertEntity>> GetFeaturedAsync(
        int count,
        CancellationToken ct = default);

    Task<IReadOnlyList<ConcertEntity>> GetUnevaluatedAsync(
        int limit,
        CancellationToken ct = default);
}
