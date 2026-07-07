namespace TxConcert.Domain.Common.Errors;

public sealed class TooManyRequestsException : DomainException
{
    public TooManyRequestsException(string message, int retryAfterSeconds = 60, string? i18nKey = null)
        : base(message, 429, i18nKey: i18nKey)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public int RetryAfterSeconds { get; }
}
