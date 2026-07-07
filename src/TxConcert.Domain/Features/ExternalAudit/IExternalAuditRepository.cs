using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.ExternalAudit;

public interface IExternalAuditRepository : IRepository<ExternalAuditEntity>
{
    Task<(IReadOnlyList<ExternalAuditEntity> Items, long Total)> GetPaginatedFilteredAsync(
        int limit,
        int offset,
        string? type = null,
        string? targetId = null,
        CancellationToken ct = default);

    Task<ExternalAuditEntity?> GetLatestByCorrelationIdAsync(
        string correlationId,
        CancellationToken ct = default);

    Task<IReadOnlyList<ExternalAuditEntity>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken ct = default);
}
