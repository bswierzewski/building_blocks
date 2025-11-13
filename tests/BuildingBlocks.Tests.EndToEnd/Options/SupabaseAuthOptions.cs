using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Supabase authentication configuration.
/// </summary>
public class SupabaseAuthOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Authentication:Supabase";

    /// <summary>
    /// Gets or sets the Supabase project URL.
    /// </summary>
    [Required(ErrorMessage = "Supabase URL configuration is required")]
    public string Url { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Supabase public API key.
    /// </summary>
    [Required(ErrorMessage = "Supabase Key configuration is required")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the test email for authentication.
    /// </summary>
    [Required(ErrorMessage = "Supabase TestEmail configuration is required")]
    public string TestEmail { get; set; } = null!;

    /// <summary>
    /// Gets or sets the test password for authentication.
    /// </summary>
    [Required(ErrorMessage = "Supabase TestPassword configuration is required")]
    public string TestPassword { get; set; } = null!;
}
