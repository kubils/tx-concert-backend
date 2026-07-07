namespace TxConcert.Domain.Common.Errors;

public sealed class BadRequestException : DomainException
{
    public BadRequestException(string message, string? i18nKey = null)
        : base(message, 400, i18nKey: i18nKey) { }
}
