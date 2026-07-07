using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Venues;

public interface IVenueRepository : IRepository<VenueEntity>
{
    Task<VenueEntity?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<(IReadOnlyList<VenueEntity> Items, long Total)> GetByStateAsync(
        string stateId,
        int limit,
        int offset,
        CancellationToken ct = default);

    Task<(IReadOnlyList<VenueEntity> Items, long Total)> SearchAsync(
        string? searchTerm,
        string? stateId,
        int limit,
        int offset,
        CancellationToken ct = default);
}
