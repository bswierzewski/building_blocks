using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Modules.Users.Web.Options;

/// <summary>
/// Configuration options for Supabase authentication provider.
/// </summary>
public class SupabaseOptions
{
    /// <summary>
    /// The configuration section name for Supabase options.
    /// </summary>
    public const string SectionName = "Authentication:Supabase";

    /// <summary>
    /// The Supabase project URL (e.g., https://your-project.supabase.co).
    /// This is the issuer of the JWT tokens and base URL for JWKS endpoint.
    /// </summary>
    [Required(ErrorMessage = "Authentication:Supabase:ProjectUrl is required")]
    [Url(ErrorMessage = "Authentication:Supabase:ProjectUrl must be a valid URL")]
    public string ProjectUrl { get; set; } = null!;

    /// <summary>
    /// The audience for JWT token validation (optional).
    /// If not set, audience validation will be disabled.
    /// Supabase typically uses 'authenticated' as the audience.
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Gets the JWKS (JSON Web Key Set) endpoint URL.
    /// Supabase exposes public keys at: {ProjectUrl}/auth/v1/.well-known/jwks.json
    /// </summary>
    public string JwksUrl => $"{ProjectUrl.TrimEnd('/')}/auth/v1/.well-known/jwks.json";
}
