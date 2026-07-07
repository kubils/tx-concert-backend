namespace TxConcert.Domain.Common.Errors;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message, string? i18nKey = null)
        : base(message, 404, i18nKey: i18nKey) { }
}
