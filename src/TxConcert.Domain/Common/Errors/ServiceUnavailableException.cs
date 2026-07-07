namespace TxConcert.Domain.Common.Errors;

public sealed class ServiceUnavailableException : DomainException
{
    public ServiceUnavailableException(string message)
        : base(message, 503) { }
}
