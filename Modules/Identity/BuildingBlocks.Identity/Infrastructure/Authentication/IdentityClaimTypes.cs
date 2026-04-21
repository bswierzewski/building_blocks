namespace BuildingBlocks.Identity.Infrastructure.Authentication;

/// <summary>
/// Shared claim names used as the contract between provider-specific JWT handlers,
/// JIT provisioning, and the current user abstraction.
/// </summary>
public static class IdentityClaimTypes
{
    /// <summary>
    /// Internal claim containing the normalized external provider name.
    /// </summary>
    public const string ExternalProvider = "identity_external_provider";

    /// <summary>
    /// Internal claim containing the provider-specific subject identifier.
    /// </summary>
    public const string ExternalUserId = "identity_external_user_id";

    /// <summary>
    /// Internal claim containing the normalized email used by JIT provisioning.
    /// </summary>
    public const string Email = "identity_email";

    /// <summary>
    /// Internal claim containing the local database identifier of the provisioned user.
    /// </summary>
    public const string LocalUserId = "local_user_id";

    /// <summary>
    /// Internal claim used to project application permissions onto the authenticated principal.
    /// </summary>
    public const string Permission = "permission";
}