using TxConcert.Application.Common.Interfaces;
using TxConcert.Domain.Common.Base;
using TxConcert.Domain.Common.Cqrs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TxConcert.Application.Common.Behaviors;

public sealed class DomainEventDispatcherBehavior<TRequest, TResponse>(
    IDbContext dbContext,
    IPublisher publisher,
    ILogger<DomainEventDispatcherBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        TResponse response = await next();

        // Collect domain events from all tracked entities
        List<IDomainEvent> domainEvents = dbContext.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        if (domainEvents.Count == 0)
            return response;

        // Clear events before dispatching to prevent re-entrancy issues
        foreach (var entry in dbContext.ChangeTracker.Entries<BaseEntity>())
            entry.Entity.ClearDomainEvents();

        // Dispatch each event via MediatR (INotification handlers)
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            logger.LogDebug("Dispatching domain event {EventType}", domainEvent.GetType().Name);
            await publisher.Publish(domainEvent, ct);
        }

        logger.LogDebug("Dispatched {Count} domain event(s) for {Request}",
            domainEvents.Count, typeof(TRequest).Name);

        return response;
    }
}
