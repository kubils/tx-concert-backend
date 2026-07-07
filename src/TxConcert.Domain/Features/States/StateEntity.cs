using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.States;

public sealed class StateEntity : BaseEntity
{
    private StateEntity() { } // EF Core

    public static StateEntity Create(
        string name,
        string abbreviation,
        string country = "US",
        bool isActive = true,
        int sortOrder = 0)
    {
        var entity = new StateEntity
        {
            Name = name,
            Abbreviation = abbreviation,
            Country = country,
            IsActive = isActive,
            SortOrder = sortOrder
        };
        entity.SetId(Constants.IdPrefix.State);
        return entity;
    }

    public string Name { get; private set; } = default!;
    public string Abbreviation { get; private set; } = default!;
    public string Country { get; private set; } = "US";
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetSortOrder(int order) => SortOrder = order;
}
