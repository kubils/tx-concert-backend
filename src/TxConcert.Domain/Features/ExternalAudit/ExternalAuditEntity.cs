using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;
using NodaTime;

namespace TxConcert.Domain.Features.ExternalAudit;

/// <summary>
/// Tracks outbound HTTP interactions (webhooks, API calls).
/// Supports retry capability and audit querying by correlation, target, and type.
/// </summary>
public sealed class ExternalAuditEntity : BaseEntity
{
    private ExternalAuditEntity() { } // EF Core

    public static ExternalAuditEntity Create(
        string correlationId,
        string type,
        string requestUrl,
        string requestBody,
        string? targetId = null)
    {
        var entity = new ExternalAuditEntity
        {
            CorrelationId = correlationId,
            Type = type,
            RequestUrl = requestUrl,
            RequestBody = requestBody,
            TargetId = targetId,
            CanRetry = false,
            Attempts = 1,
            Latest = false,
            ResponseTime = 0
        };
        entity.SetId(Constants.IdPrefix.ExternalAudit);
        return entity;
    }

    public string CorrelationId { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public string RequestUrl { get; private set; } = default!;
    public string RequestBody { get; private set; } = default!;
    public string? ResponseBody { get; private set; }
    public int? StatusCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool CanRetry { get; private set; }
    public string? TargetId { get; private set; }
    public int Attempts { get; private set; }
    public bool Latest { get; private set; }
    public long ResponseTime { get; private set; } // milliseconds

    public void SetResponse(string? responseBody, int statusCode, long responseTimeMs)
    {
        ResponseBody = responseBody;
        StatusCode = statusCode;
        ResponseTime = responseTimeMs;
    }

    public void SetError(string errorMessage, int? statusCode = null)
    {
        ErrorMessage = errorMessage;
        if (statusCode.HasValue) StatusCode = statusCode;
    }

    public void MarkAsLatest() => Latest = true;

    public void EnableRetry(string jobId)
    {
        CanRetry = !string.IsNullOrEmpty(jobId);
    }

    public void IncrementAttempts() => Attempts++;

    public void SetTargetId(string targetId) => TargetId = targetId;
}
