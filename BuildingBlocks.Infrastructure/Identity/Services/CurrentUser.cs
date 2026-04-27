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

    public string Id => Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public string Email => Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public IReadOnlySet<string> Roles =>
        Principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<string> Permissions =>
        Principal.FindAll(PermissionClaimsTransformation.PermissionClaimType).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) =>
        Principal.HasClaim(PermissionClaimsTransformation.PermissionClaimType, permission);
}
