using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Clerk authentication configuration for E2E tests.
/// </summary>
public class ClerkOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Clerk";

    /// <summary>
    /// Gets or sets the Clerk test token.
    /// </summary>
    [Required(ErrorMessage = "TestToken is required")]
    public string TestToken { get; set; } = null!;
}
