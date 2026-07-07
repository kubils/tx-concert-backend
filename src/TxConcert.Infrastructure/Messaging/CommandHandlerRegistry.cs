using System.Collections.Frozen;
using System.Text.Json;
using TxConcert.Domain.Common.Cqrs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TxConcert.Infrastructure.Messaging;

/// <summary>
/// Maps command name strings to their CLR types and dispatches deserialized commands via MediatR.
/// Used by CommandMessageConsumer to resolve the correct handler for outbox-queued commands.
/// NestJS equivalent: CommandHandler.register() + CommandBusService dispatch logic.
/// </summary>
public sealed class CommandHandlerRegistry
{
    private readonly Dictionary<string, Type> _registrations = new(StringComparer.OrdinalIgnoreCase);
    private FrozenDictionary<string, Type>? _frozen;

    /// <summary>Registers a command type under the given name. Call during startup.</summary>
    public void Register<T>(string commandName) where T : class, ICommand
    {
        _registrations[commandName] = typeof(T);
        _frozen = null; // Invalidate frozen cache
    }

    /// <summary>Deserializes and sends the command to its MediatR handler.</summary>
    public async Task HandleAsync(
        string commandName,
        string jsonPayload,
        IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        FrozenDictionary<string, Type> map = _frozen ??= _registrations.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        if (!map.TryGetValue(commandName, out Type? commandType))
        {
            ILogger<CommandHandlerRegistry> logger = serviceProvider
                .GetRequiredService<ILogger<CommandHandlerRegistry>>();
            logger.LogWarning("No handler registered for command {CommandName}", commandName);
            return;
        }

        object command = JsonSerializer.Deserialize(jsonPayload, commandType)
            ?? throw new InvalidOperationException($"Failed to deserialize command '{commandName}'");

        ISender sender = serviceProvider.GetRequiredService<ISender>();
        await sender.Send(command, ct);
    }
}
