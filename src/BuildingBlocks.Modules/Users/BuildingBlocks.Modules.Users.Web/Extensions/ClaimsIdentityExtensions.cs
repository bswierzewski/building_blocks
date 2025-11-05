using System.Security.Claims;
using BuildingBlocks.Application.Security;
using BuildingBlocks.Modules.Users.Domain.Aggregates;

namespace BuildingBlocks.Modules.Users.Web.Extensions;

/// <summary>
/// Extension methods for enriching ClaimsIdentity with user data.
/// </summary>
public static class ClaimsIdentityExtensions
{
    /// <summary>
    /// Enriches the claims identity with user ID, roles, and permissions from the database.
    /// </summary>
    /// <param name="identity">The claims identity to enrich</param>
    /// <param name="user">The user with roles and permissions loaded</param>
    public static void EnrichWithUserClaims(this ClaimsIdentity identity, User user)
    {
        if (identity == null)
            throw new ArgumentNullException(nameof(identity));

        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Add internal user ID (GUID from database) as custom claim
        // ClaimTypes.NameIdentifier remains unchanged with external ID from JWT 'sub' claim
        identity.AddClaim(new Claim(CustomClaimTypes.UserId, user.Id.Value.ToString()));

        // Add roles from database (using standard ClaimTypes.Role)
        foreach (var role in user.Roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
        }

        // Add permissions from database (using custom permission claim)
        foreach (var permission in user.GetAllPermissions())
        {
            identity.AddClaim(new Claim(CustomClaimTypes.Permission, permission.Name));
        }
    }
}
