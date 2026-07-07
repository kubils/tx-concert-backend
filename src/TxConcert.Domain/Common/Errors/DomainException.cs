namespace TxConcert.Domain.Common.Errors;

/// <summary>
/// Base class for all domain-specific exceptions.
/// Maps to HTTP status codes and supports i18n and structured logging.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(
        string message,
        int statusCode,
        string? logMessage = null,
        Dictionary<string, string>? logContext = null,
        string? i18nKey = null)
        : base(message)
    {
        StatusCode = statusCode;
        LogMessage = logMessage ?? message;
        LogContext = logContext ?? [];
        I18nKey = i18nKey;
    }

    public int StatusCode { get; }
    public string LogMessage { get; }
    public Dictionary<string, string> LogContext { get; }
    public string? I18nKey { get; }

    protected void AddToLogContext(string key, string value)
        => LogContext[key] = value;
}
