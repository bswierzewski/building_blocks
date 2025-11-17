using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Attributes;

namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Supabase authentication configuration for E2E tests.
/// </summary>
public class SupabaseOptions : IOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public static string SectionName => "Supabase";

    /// <summary>
    /// Gets or sets the Supabase project URL.
    /// </summary>
    [Required(ErrorMessage = "Url is required")]
    [EnvVariable(Description = "Supabase project URL")]
    public string Url { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Supabase public API key.
    /// </summary>
    [Required(ErrorMessage = "Key is required")]
    [EnvVariable(Description = "Supabase public API key", Sensitive = true)]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the test email for authentication.
    /// </summary>
    [Required(ErrorMessage = "TestEmail is required")]
    [EnvVariable(Description = "Test user email for E2E tests")]
    public string TestEmail { get; set; } = null!;

    /// <summary>
    /// Gets or sets the test password for authentication.
    /// </summary>
    [Required(ErrorMessage = "TestPassword is required")]
    [EnvVariable(Description = "Test user password for E2E tests", Sensitive = true)]
    public string TestPassword { get; set; } = null!;
}
