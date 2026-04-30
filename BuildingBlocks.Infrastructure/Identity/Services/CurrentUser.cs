using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Infrastructure.Identity.Services;

/// <summary>
/// Reads the current user context directly from JWT claims.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public string Id => Principal.FindFirstValue(ClaimNames.Sub) ?? string.Empty;

    public string Email => Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public IReadOnlySet<string> Roles =>
        Principal.FindAll(ClaimNames.Roles).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<string> Permissions =>
        Principal.FindAll(ClaimNames.Permission).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) =>
        Principal.HasClaim(ClaimNames.Permission, permission);
}
