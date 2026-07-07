using TxConcert.Domain.Common.Base;
using TxConcert.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementing IRepository for all BaseEntity-derived types.
/// NestJS equivalent: src/infrastructure/persistence/base/repository.ts
/// </summary>
public class BaseRepository<T>(ApplicationDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyList<T>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default)
    {
        List<string> idList = ids.ToList();
        return await DbSet.Where(e => idList.Contains(e.Id)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet.ToListAsync(ct);
    }

    public async Task<PaginatedResult<T>> GetPaginatedAsync(int limit, int offset, CancellationToken ct = default)
    {
        long totalCount = await DbSet.LongCountAsync(ct);
        IReadOnlyList<T> items = await DbSet
            .OrderByDescending(e => e.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);
        return new PaginatedResult<T>(items, totalCount, limit, offset);
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(e => e.Id == id, ct);
    }

    public async Task UpsertAsync(T entity, CancellationToken ct = default)
    {
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> entry = Context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            T? existing = await DbSet.FindAsync([entity.Id], ct);
            if (existing is null)
                await DbSet.AddAsync(entity, ct);
            else
                entry.State = EntityState.Modified;
        }
        await Context.SaveChangesAsync(ct);
    }

    public async Task BulkUpsertAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        foreach (T entity in entities)
        {
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> entry = Context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                T? existing = await DbSet.FindAsync([entity.Id], ct);
                if (existing is null)
                    await DbSet.AddAsync(entity, ct);
                else
                    entry.State = EntityState.Modified;
            }
        }
        await Context.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(T entity, CancellationToken ct = default)
    {
        entity.SoftDelete();
        Context.Entry(entity).State = EntityState.Modified;
        await Context.SaveChangesAsync(ct);
    }

    public async Task HardDeleteAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync(ct);
    }
}
