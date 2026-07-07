namespace TxConcert.Application.Common.Interfaces;

/// <summary>
/// Provides the current authenticated user's identity from the HTTP context.
/// NestJS equivalent: currentUser from JWT payload injected via decorator.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? DisplayName { get; }
    bool IsAuthenticated { get; }
}
