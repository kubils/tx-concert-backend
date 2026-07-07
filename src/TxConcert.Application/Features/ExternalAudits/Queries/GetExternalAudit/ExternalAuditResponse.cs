using TxConcert.Domain.Features.ExternalAudit;
using NodaTime;

namespace TxConcert.Application.Features.ExternalAudits.Queries.GetExternalAudit;

public sealed record ExternalAuditResponse(
    string Id,
    string CorrelationId,
    string Type,
    string RequestUrl,
    string RequestBody,
    string? ResponseBody,
    int? StatusCode,
    string? ErrorMessage,
    bool CanRetry,
    string? TargetId,
    int Attempts,
    bool Latest,
    long ResponseTimeMs,
    Instant CreatedAt,
    Instant UpdatedAt)
{
    public static ExternalAuditResponse From(ExternalAuditEntity e) => new(
        e.Id,
        e.CorrelationId,
        e.Type,
        e.RequestUrl,
        e.RequestBody,
        e.ResponseBody,
        e.StatusCode,
        e.ErrorMessage,
        e.CanRetry,
        e.TargetId,
        e.Attempts,
        e.Latest,
        e.ResponseTime,
        e.CreatedAt,
        e.UpdatedAt);
}
