using TxConcert.Application.Common.Interfaces;
using TxConcert.Domain.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TxConcert.Infrastructure.Services;

/// <summary>
/// Extracts current user from the HTTP context claims.
/// NestJS equivalent: currentUser from JWT payload via @CurrentUser() decorator.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirst(Constants.Jwt.UserIdClaim)?.Value;

    public string? DisplayName =>
        httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
