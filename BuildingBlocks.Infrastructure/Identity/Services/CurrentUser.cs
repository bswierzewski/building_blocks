using System.Security.Claims;
using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Identity.Services;

/// <summary>
/// Reads the current user context directly from JWT claims.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public string Id => string.Empty;

    public string Email => string.Empty;

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public IReadOnlySet<string> Roles => new HashSet<string>();

    public IReadOnlySet<string> Permissions => new HashSet<string>();

    public bool HasPermission(string permission) => false;
}