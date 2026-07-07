using TxConcert.Domain.Features.ExternalAudit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TxConcert.Application.Features.ExternalAudits.Commands.CreateRequestAudit;

public sealed class CreateRequestAuditHandler(
    IExternalAuditRepository repository,
    ILogger<CreateRequestAuditHandler> logger)
    : IRequestHandler<CreateRequestAuditCommand, string>
{
    public async Task<string> Handle(CreateRequestAuditCommand request, CancellationToken ct)
    {
        logger.LogInformation(
            "Creating external audit for correlation {CorrelationId}, type {Type}, target {TargetId}, status {StatusCode}",
            request.CorrelationId,
            request.Type,
            request.TargetId,
            request.StatusCode);

        ExternalAuditEntity entity = ExternalAuditEntity.Create(
            request.CorrelationId,
            request.Type,
            request.RequestUrl,
            request.RequestBody,
            request.TargetId);

        if (request.StatusCode.HasValue)
            entity.SetResponse(request.ResponseBody, request.StatusCode.Value, request.ResponseTimeMs);

        if (request.ErrorMessage is not null)
            entity.SetError(request.ErrorMessage, request.StatusCode);

        entity.MarkAsLatest();

        await repository.UpsertAsync(entity, ct);
        logger.LogInformation(
            "Created external audit {AuditId} for correlation {CorrelationId}",
            entity.Id,
            request.CorrelationId);
        return entity.Id;
    }
}
