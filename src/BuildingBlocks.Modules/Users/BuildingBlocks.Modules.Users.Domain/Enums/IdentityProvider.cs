namespace BuildingBlocks.Modules.Users.Domain.Enums;

/// <summary>
/// Represents the external identity provider that authenticated the user.
/// Supports multiple authentication providers for flexible integration.
/// </summary>
public enum IdentityProvider
{
    /// <summary>
    /// Unknown or unrecognized provider
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Auth0 identity provider (https://auth0.com)
    /// </summary>
    Auth0 = 1,

    /// <summary>
    /// Clerk identity provider (https://clerk.com)
    /// </summary>
    Clerk = 2,

    /// <summary>
    /// Microsoft Azure Active Directory
    /// </summary>
    AzureAD = 3,

    /// <summary>
    /// Google authentication
    /// </summary>
    Google = 4,

    /// <summary>
    /// Keycloak identity provider
    /// </summary>
    Keycloak = 5,

    /// <summary>
    /// Custom or other identity provider
    /// </summary>
    Custom = 99
}
