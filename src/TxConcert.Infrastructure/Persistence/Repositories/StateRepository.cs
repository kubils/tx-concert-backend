using TxConcert.Domain.Features.States;
using Microsoft.EntityFrameworkCore;

namespace TxConcert.Infrastructure.Persistence.Repositories;

public sealed class StateRepository(ApplicationDbContext context)
    : BaseRepository<StateEntity>(context), IStateRepository
{
    public async Task<StateEntity?> GetByAbbreviationAsync(string abbreviation, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Abbreviation == abbreviation, ct);
    }

    public async Task<IReadOnlyList<StateEntity>> GetActiveStatesAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.IsActive)
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .ToListAsync(ct);
    }
}
