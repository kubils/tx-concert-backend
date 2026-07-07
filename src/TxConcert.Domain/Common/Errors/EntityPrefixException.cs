namespace TxConcert.Domain.Common.Errors;

public sealed class EntityPrefixException : OperationException
{
    public EntityPrefixException(string prefix)
        : base(
            $"Entity ID prefix '{prefix}' exceeds the maximum length of 5 characters.",
            $"EntityPrefixError: prefix='{prefix}' length={prefix.Length}") { }
}
