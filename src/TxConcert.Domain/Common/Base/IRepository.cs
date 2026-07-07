namespace TxConcert.Domain.Common.Base;

/// <summary>Generic repository interface for all domain entities.</summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<PaginatedResult<T>> GetPaginatedAsync(int limit, int offset, CancellationToken ct = default);
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
    Task UpsertAsync(T entity, CancellationToken ct = default);
    Task BulkUpsertAsync(IEnumerable<T> entities, CancellationToken ct = default);
    Task RemoveAsync(T entity, CancellationToken ct = default);       // soft delete
    Task HardDeleteAsync(T entity, CancellationToken ct = default);   // permanent delete
}
