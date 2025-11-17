using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Attributes;

namespace BuildingBlocks.Modules.Users.Application.Options;

/// <summary>
/// Configuration options for Supabase authentication provider.
/// </summary>
public class SupabaseOptions : IOptions
{
    /// <summary>
    /// The configuration section name for Supabase options.
    /// </summary>
    public static string SectionName => "Supabase";

    /// <summary>
    /// The Supabase project URL (e.g., https://your-project.supabase.co).
    /// This is the issuer of the JWT tokens and base URL for JWKS endpoint.
    /// </summary>
    [Required(ErrorMessage = "Authority is required")]
    [Url(ErrorMessage = "Authority must be a valid URL")]
    [EnvVariable(Description = "Supabase project URL (e.g., https://your-project.supabase.co)")]
    public string Authority { get; set; } = null!;

    /// <summary>
    /// The audience for JWT token validation (optional).
    /// If not set, audience validation will be disabled.
    /// Supabase typically uses 'authenticated' as the audience.
    /// </summary>
    [EnvVariable(Description = "Supabase audience for JWT validation", Required = false)]
    public string? Audience { get; set; }

    /// <summary>
    /// The JWT secret key for HS256 signature validation.
    /// This is the secret key from your Supabase project settings.
    /// </summary>
    [Required(ErrorMessage = "JwtSecret is required")]
    [EnvVariable(Description = "Supabase JWT secret key", Sensitive = true)]
    public string JwtSecret { get; set; } = null!;
}
