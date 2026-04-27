using System.Security.Claims;

namespace BuildingBlocks.Infrastructure.Identity;

/// <summary>
/// Centralizes claim types used by the application identity pipeline.
/// </summary>
public static class IdentityClaimTypes
{
    public const string Subject = "sub";
    public const string Email = "email";
    public const string Role = "role";
    public const string Roles = "roles";
    public const string Permission = "permission";
    public const string Permissions = "permissions";

    public static readonly string[] UserIdClaims = [ClaimTypes.NameIdentifier, Subject];

    public static readonly string[] EmailClaims = [ClaimTypes.Email, Email];

    public static readonly string[] RoleClaims = [ClaimTypes.Role, Role, Roles];

    public static readonly string[] PermissionClaims = [Permissions, Permission];
}