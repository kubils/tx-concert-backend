namespace TxConcert.Application.Common.Interfaces;

/// <summary>
/// Marker interface. Commands/queries implementing this will be wrapped
/// in a database transaction by TransactionBehavior.
/// </summary>
public interface ITransactional;
