using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TxConcert.Application.Common.Interfaces;

/// <summary>
/// Abstraction over ApplicationDbContext for use in Application layer.
/// Keeps Application decoupled from EF Core implementation details.
/// </summary>
public interface IDbContext
{
    DbSet<T> Set<T>() where T : class;
    ChangeTracker ChangeTracker { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
