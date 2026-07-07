using TxConcert.Domain.Common.Cqrs;
using TxConcert.Domain.Common.Errors;
using NodaTime;

namespace TxConcert.Domain.Common.Base;

/// <summary>
/// Base class for all domain entities.
/// Provides UUID generation with prefix validation, soft delete, audit fields, and domain event collection.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>EF Core requires a parameterless constructor. Do NOT use in domain code — use static Create() factories.</summary>
    protected BaseEntity() { }

    private readonly List<IDomainEvent> _domainEvents = [];

    public string Id { get; private set; } = default!;
    public Instant CreatedAt { get; private set; }
    public Instant UpdatedAt { get; private set; }
    public Instant? DeletedAt { get; private set; }

    public string? CreatedById { get; private set; }
    public string? CreatedByName { get; private set; }
    public string? ModifiedById { get; private set; }
    public string? ModifiedByName { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    /// <summary>Domain events raised by this entity, dispatched after SaveChanges.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Raises a domain event to be dispatched after persistence.</summary>
    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    /// <summary>Clears all domain events. Called by DomainEventDispatcherBehavior after dispatch.</summary>
    internal void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Generates and sets the entity ID with a typed prefix.
    /// Prefix must be max 5 characters (e.g. "cdout", "exaud").
    /// Throws <see cref="EntityPrefixException"/> if prefix exceeds 5 chars.
    /// </summary>
    protected void SetId(string prefix)
    {
        if (prefix.Length > 5)
            throw new EntityPrefixException(prefix);

        Id = $"{prefix}_{Guid.NewGuid():N}";
    }

    /// <summary>Called by AuditInterceptor on entity creation.</summary>
    internal void SetCreatedAt(Instant value) => CreatedAt = value;

    /// <summary>Called by AuditInterceptor on entity modification.</summary>
    internal void SetUpdatedAt(Instant value) => UpdatedAt = value;

    /// <summary>Sets audit fields (createdBy on first call, modifiedBy on subsequent).</summary>
    public void SetAuditFields(string userId, string? displayName = null)
    {
        if (CreatedById is null)
        {
            CreatedById = userId;
            CreatedByName = displayName;
        }

        ModifiedById = userId;
        ModifiedByName = displayName;
    }

    /// <summary>Soft-deletes the entity. Filtered out by EF global query filter.</summary>
    public void SoftDelete()
    {
        DeletedAt = SystemClock.Instance.GetCurrentInstant();
    }
}
