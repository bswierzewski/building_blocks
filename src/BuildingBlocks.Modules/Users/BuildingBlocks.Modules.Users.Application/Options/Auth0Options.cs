using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Attributes;

namespace BuildingBlocks.Modules.Users.Application.Options;

/// <summary>
/// Configuration options for Auth0 authentication provider.
/// </summary>
public class Auth0Options : IOptions
{
    /// <summary>
    /// The configuration section name for Auth0 options.
    /// </summary>
    public static string SectionName => "Auth0";

    /// <summary>
    /// The authority URL for Auth0 (e.g., https://your-tenant.auth0.com/).
    /// This is the issuer of the JWT tokens.
    /// </summary>
    [Required(ErrorMessage = "Authority is required")]
    [Url(ErrorMessage = "Authority must be a valid URL")]
    [EnvVariable(Description = "Auth0 authority URL (e.g., https://your-tenant.auth0.com/)")]
    public string Authority { get; set; } = null!;

    /// <summary>
    /// The audience for JWT token validation (required for Auth0).
    /// This is typically your API identifier in Auth0.
    /// </summary>
    [Required(ErrorMessage = "Audience is required")]
    [EnvVariable(Description = "Auth0 API audience identifier")]
    public string Audience { get; set; } = null!;
}
