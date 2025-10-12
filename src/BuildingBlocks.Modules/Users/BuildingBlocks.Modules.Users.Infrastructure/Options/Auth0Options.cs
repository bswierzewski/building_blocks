using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Modules.Users.Infrastructure.Options;

/// <summary>
/// Configuration options for Auth0 authentication provider.
/// </summary>
public class Auth0Options
{
    /// <summary>
    /// The configuration section name for Auth0 options.
    /// </summary>
    public const string SectionName = "Auth0";

    /// <summary>
    /// The authority URL for Auth0 (e.g., https://your-tenant.auth0.com/).
    /// This is the issuer of the JWT tokens.
    /// </summary>
    [Required(ErrorMessage = "Auth0:Authority is required")]
    [Url(ErrorMessage = "Auth0:Authority must be a valid URL")]
    public string Authority { get; set; } = null!;

    /// <summary>
    /// The audience for JWT token validation (required for Auth0).
    /// This is typically your API identifier in Auth0.
    /// </summary>
    [Required(ErrorMessage = "Auth0:Audience is required")]
    public string Audience { get; set; } = null!;
}
