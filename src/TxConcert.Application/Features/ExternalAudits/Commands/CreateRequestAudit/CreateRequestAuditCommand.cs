using TxConcert.Application.Common.Interfaces;
using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Application.Features.ExternalAudits.Commands.CreateRequestAudit;

/// <summary>
/// Creates a new external audit record for an outbound HTTP call.
/// Wrapped in a transaction (implements ITransactional).
/// </summary>
public sealed record CreateRequestAuditCommand(
    string CorrelationId,
    string Type,
    string RequestUrl,
    string RequestBody,
    string? ResponseBody,
    int? StatusCode,
    string? ErrorMessage,
    long ResponseTimeMs,
    string? TargetId = null) : ICommand<string>, ITransactional;
