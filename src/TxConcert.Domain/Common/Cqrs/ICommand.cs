using MediatR;

namespace TxConcert.Domain.Common.Cqrs;

/// <summary>
/// Marker interface for commands (write operations) without a return value.
/// Provides compile-time separation between commands, queries, and events.
/// </summary>
public interface ICommand : IRequest;

/// <summary>
/// Marker interface for commands that return a response.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;
