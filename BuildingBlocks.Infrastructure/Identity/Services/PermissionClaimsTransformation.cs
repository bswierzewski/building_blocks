using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace BuildingBlocks.Infrastructure.Identity.Services;

/// <summary>
/// ASP.NET Core claims transformation that appends permission claims to the principal
/// based on the roles already present in the identity.
/// </summary>
public sealed class PermissionClaimsTransformation(RolePermissionService rolePermissionService) : IClaimsTransformation
{
    /// <summary>The claim type used to carry individual permission codes.</summary>
    public const string PermissionClaimType = "permission";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
        var permissions = rolePermissionService.GetPermissionsForRoles(roles);

        var identity = new ClaimsIdentity();

        foreach (var permission in permissions)
        {
            if (!principal.HasClaim(PermissionClaimType, permission))
                identity.AddClaim(new Claim(PermissionClaimType, permission));
        }

        if (identity.Claims.Any())
            principal.AddIdentity(identity);

        return Task.FromResult(principal);
    }
}
