namespace TxConcert.Domain.Common.Errors;

public class UnprocessableEntityException : DomainException
{
    public UnprocessableEntityException(string message, string? i18nKey = null)
        : base(message, 422, i18nKey: i18nKey) { }
}
