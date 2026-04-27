using System.Security.Claims;
using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Identity;

/// <summary>
/// Reads the current user context directly from JWT claims.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor, RolePermissionMap rolePermissionMap) : ICurrentUser
{
    private IReadOnlySet<string>? roles;
    private IReadOnlySet<string>? permissions;

    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public Guid Id => TryGetClaimValue(IdentityClaimTypes.UserIdClaims, out var userId) && Guid.TryParse(userId, out var parsedUserId)
        ? parsedUserId
        : Guid.Empty;

    public string Email => TryGetClaimValue(IdentityClaimTypes.EmailClaims, out var email) ? email : string.Empty;

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public IReadOnlySet<string> Roles => roles ??= GetClaims(IdentityClaimTypes.RoleClaims);

    public IReadOnlySet<string> Permissions => permissions ??= rolePermissionMap.GetPermissions(Roles);

    public bool HasPermission(string permission) => !string.IsNullOrWhiteSpace(permission) && Permissions.Contains(permission.Trim());

    private IReadOnlySet<string> GetClaims(IEnumerable<string> claimTypes)
    {
        return Principal.Claims
            .Where(claim => claimTypes.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private bool TryGetClaimValue(IEnumerable<string> claimTypes, out string value)
    {
        value = Principal.Claims
            .FirstOrDefault(claim => claimTypes.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))?
            .Value
            ?? string.Empty;

        return !string.IsNullOrWhiteSpace(value);
    }
}