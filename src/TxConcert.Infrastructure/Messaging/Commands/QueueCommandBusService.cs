using TxConcert.Domain.Commands;
using TxConcert.Domain.Common.Cqrs;
using TxConcert.Infrastructure.Persistence;
using System.Text.Json;

namespace TxConcert.Infrastructure.Messaging.Commands;

public sealed class QueueCommandBusService(ApplicationDbContext context) : ICommandBus
{
    public async Task SendAsync<T>(T command, CancellationToken ct = default) where T : class, ICommand
    {
        string commandName = typeof(T).Name;
        CommandOutboxEntity outbox = CommandOutboxEntity.Create(
            commandName,
            JsonSerializer.Serialize(command));

        await context.CommandOutbox.AddAsync(outbox, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task SendManyAsync<T>(IEnumerable<T> commands, CancellationToken ct = default) where T : class, ICommand
    {
        string commandName = typeof(T).Name;
        IEnumerable<CommandOutboxEntity> entities = commands.Select(cmd =>
            CommandOutboxEntity.Create(commandName, JsonSerializer.Serialize(cmd)));

        await context.CommandOutbox.AddRangeAsync(entities, ct);
        await context.SaveChangesAsync(ct);
    }
}
