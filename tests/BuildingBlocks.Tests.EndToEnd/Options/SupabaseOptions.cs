using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Supabase authentication configuration for E2E tests.
/// </summary>
public class SupabaseOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Supabase";

    /// <summary>
    /// Gets or sets the Supabase project URL.
    /// </summary>
    [Required(ErrorMessage = "Url is required")]
    public string Url { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Supabase public API key.
    /// </summary>
    [Required(ErrorMessage = "Key is required")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the test email for authentication.
    /// </summary>
    [Required(ErrorMessage = "TestEmail is required")]
    public string TestEmail { get; set; } = null!;

    /// <summary>
    /// Gets or sets the test password for authentication.
    /// </summary>
    [Required(ErrorMessage = "TestPassword is required")]
    public string TestPassword { get; set; } = null!;
}
