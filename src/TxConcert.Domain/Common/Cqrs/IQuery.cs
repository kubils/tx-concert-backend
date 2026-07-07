using MediatR;

namespace TxConcert.Domain.Common.Cqrs;

/// <summary>
/// Marker interface for queries (read operations).
/// All queries must return a response of type TResponse.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
