namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Configuration options for authentication providers in E2E tests.
/// These options are populated from environment variables.
/// </summary>
public class AuthOptions
{
    /// <summary>
    /// Gets or sets the authentication provider type (e.g., "supabase", "clerk").
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// Supabase-specific configuration options.
    /// </summary>
    public SupabaseAuthOptions Supabase { get; set; } = new();

    /// <summary>
    /// Clerk-specific configuration options.
    /// </summary>
    public ClerkAuthOptions Clerk { get; set; } = new();
}
