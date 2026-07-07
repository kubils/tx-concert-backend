namespace TxConcert.Domain.Common.Errors;

public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message, string? i18nKey = null)
        : base(message, 401, i18nKey: i18nKey) { }
}
