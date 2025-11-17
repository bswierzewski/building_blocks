using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Attributes;

namespace BuildingBlocks.Modules.Users.Application.Options;

/// <summary>
/// Configuration options for Clerk authentication provider.
/// </summary>
public class ClerkOptions : IOptions
{
    /// <summary>
    /// The configuration section name for Clerk options.
    /// </summary>
    public static string SectionName => "Clerk";

    /// <summary>
    /// The authority URL for Clerk (e.g., https://your-domain.clerk.accounts.dev).
    /// This is the issuer of the JWT tokens.
    /// </summary>
    [Required(ErrorMessage = "Authority is required")]
    [Url(ErrorMessage = "Authority must be a valid URL")]
    [EnvVariable(Description = "Clerk authority URL (e.g., https://your-domain.clerk.accounts.dev)")]
    public string Authority { get; set; } = null!;

    /// <summary>
    /// The audience for JWT token validation (optional).
    /// If not set, audience validation will be disabled.
    /// </summary>
    [EnvVariable(Description = "Clerk audience for JWT validation", Required = false)]
    public string? Audience { get; set; }
}
