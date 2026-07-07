using TxConcert.Domain.Common.Errors;
using TxConcert.Domain.Features.ExternalAudit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TxConcert.Application.Features.ExternalAudits.Commands.RetryAudit;

public sealed class RetryAuditHandler(
    IExternalAuditRepository repository,
    ILogger<RetryAuditHandler> logger)
    : IRequestHandler<RetryAuditCommand>
{
    public async Task Handle(RetryAuditCommand request, CancellationToken ct)
    {
        logger.LogInformation("Retry requested for external audit {AuditId}", request.AuditId);

        ExternalAuditEntity entity = await repository.GetByIdAsync(request.AuditId, ct)
            ?? throw new NotFoundException($"External audit '{request.AuditId}' not found.");

        if (!entity.CanRetry)
        {
            logger.LogWarning("External audit {AuditId} is not eligible for retry", request.AuditId);
            throw new BadRequestException($"External audit '{request.AuditId}' is not eligible for retry.");
        }

        entity.IncrementAttempts();
        await repository.UpsertAsync(entity, ct);
        logger.LogInformation(
            "External audit {AuditId} marked for retry; total attempts is now {Attempts}",
            entity.Id,
            entity.Attempts);
    }
}
