using System.Collections.Frozen;
using System.Text.Json;
using TxConcert.Domain.Common.Cqrs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TxConcert.Infrastructure.Messaging;

/// <summary>
/// Maps event name strings to their CLR types and dispatches deserialized events via MediatR.
/// Used by EventMessageConsumer to resolve the correct handlers for outbox-queued events.
/// NestJS equivalent: EventSubscriber.register() + EventBusService dispatch logic.
/// </summary>
public sealed class EventSubscriberRegistry
{
    private readonly Dictionary<string, Type> _registrations = new(StringComparer.OrdinalIgnoreCase);
    private FrozenDictionary<string, Type>? _frozen;

    /// <summary>Registers an event type under the given name. Call during startup.</summary>
    public void Register<T>(string eventName) where T : class, IDomainEvent
    {
        _registrations[eventName] = typeof(T);
        _frozen = null;
    }

    /// <summary>Deserializes and publishes the event to all MediatR notification handlers.</summary>
    public async Task HandleAsync(
        string eventName,
        string jsonPayload,
        IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        FrozenDictionary<string, Type> map = _frozen ??= _registrations.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        if (!map.TryGetValue(eventName, out Type? eventType))
        {
            ILogger<EventSubscriberRegistry> logger = serviceProvider
                .GetRequiredService<ILogger<EventSubscriberRegistry>>();
            logger.LogWarning("No subscriber registered for event {EventName}", eventName);
            return;
        }

        object @event = JsonSerializer.Deserialize(jsonPayload, eventType)
            ?? throw new InvalidOperationException($"Failed to deserialize event '{eventName}'");

        IPublisher publisher = serviceProvider.GetRequiredService<IPublisher>();
        await publisher.Publish(@event, ct);
    }
}
