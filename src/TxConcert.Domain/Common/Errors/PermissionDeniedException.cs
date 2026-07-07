namespace TxConcert.Domain.Common.Errors;

public sealed class PermissionDeniedException : DomainException
{
    public PermissionDeniedException(string message, string? i18nKey = null)
        : base(message, 403, i18nKey: i18nKey) { }
}
