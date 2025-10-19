namespace BuildingBlocks.Application.Security;

/// <summary>
/// Defines custom claim types used throughout the application.
/// These extend the standard System.Security.Claims.ClaimTypes with application-specific claims.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    /// Internal user ID claim (GUID from database).
    /// This is the primary identifier for users in the system after JIT provisioning.
    /// Standard ClaimTypes.NameIdentifier is used for external identity provider IDs.
    /// </summary>
    public const string UserId = "user_id";

    /// <summary>
    /// Permission claim for fine-grained authorization.
    /// Multiple permission claims can exist for a single user.
    /// Example values: "users:read", "users:write", "posts:delete"
    /// </summary>
    public const string Permission = "permission";
}
