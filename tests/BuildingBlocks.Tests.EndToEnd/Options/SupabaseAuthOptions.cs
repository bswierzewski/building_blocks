namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Supabase authentication configuration.
/// </summary>
public class SupabaseAuthOptions
{
    /// <summary>
    /// Gets or sets the Supabase project URL.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Supabase public API key.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the test email for authentication.
    /// </summary>
    public string TestEmail { get; set; } = null!;

    /// <summary>
    /// Gets or sets the test password for authentication.
    /// </summary>
    public string TestPassword { get; set; } = null!;
}
