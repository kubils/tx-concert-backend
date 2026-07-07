namespace TxConcert.Domain.Common.Errors;

public sealed class ConflictException : DomainException
{
    public ConflictException(string message, string? i18nKey = null)
        : base(message, 409, i18nKey: i18nKey) { }
}
