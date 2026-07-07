using TxConcert.Domain.Commands;
using TxConcert.Infrastructure.Messaging.Commands;
using TxConcert.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TxConcert.Infrastructure.Messaging.Consumers;

/// <summary>
/// MassTransit consumer for processing commands from RabbitMQ.
/// Resolves the handler via CommandHandlerRegistry and updates the outbox record.
/// NestJS equivalent: BullMQ worker processor for commands.
/// </summary>
public sealed class CommandMessageConsumer(
    IServiceScopeFactory scopeFactory,
    CommandHandlerRegistry registry,
    ILogger<CommandMessageConsumer> logger)
    : IConsumer<CommandMessage>
{
    public async Task Consume(ConsumeContext<CommandMessage> context)
    {
        CommandMessage message = context.Message;
        logger.LogInformation("Processing command {Command} (outbox: {OutboxId})",
            message.Command, message.OutboxId);

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        CommandOutboxEntity? outbox = await db.CommandOutbox.FindAsync([message.OutboxId], context.CancellationToken);
        if (outbox is null)
        {
            logger.LogWarning("Command outbox {OutboxId} not found, skipping", message.OutboxId);
            return;
        }

        try
        {
            await registry.HandleAsync(message.Command, message.Content, scope.ServiceProvider, context.CancellationToken);
            outbox.SetSuccess();
            logger.LogInformation("Command {CommandName} processed successfully", message.Command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Command {CommandName} failed", message.Command);
            outbox.SetFailed(ex.Message);
        }

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
