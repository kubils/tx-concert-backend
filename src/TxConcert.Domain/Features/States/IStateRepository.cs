using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.States;

public interface IStateRepository : IRepository<StateEntity>
{
    Task<StateEntity?> GetByAbbreviationAsync(string abbreviation, CancellationToken ct = default);
    Task<IReadOnlyList<StateEntity>> GetActiveStatesAsync(CancellationToken ct = default);
}
