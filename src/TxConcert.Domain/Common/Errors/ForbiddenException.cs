namespace TxConcert.Domain.Common.Errors;

public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message, string? i18nKey = null)
        : base(message, 403, i18nKey: i18nKey) { }
}
