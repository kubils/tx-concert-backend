namespace TxConcert.Domain.Common.Errors;

public sealed class ConfigurationException : OperationException
{
    public ConfigurationException(string message)
        : base(message, $"Configuration error: {message}") { }
}
