namespace TxConcert.Domain.Common.Errors;

public class OperationException : DomainException
{
    public OperationException(string message, string? logMessage = null)
        : base(message, 500, logMessage) { }
}
