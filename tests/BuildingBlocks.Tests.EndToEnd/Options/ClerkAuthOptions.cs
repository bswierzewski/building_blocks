using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Clerk authentication configuration.
/// </summary>
public class ClerkAuthOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Authentication:Clerk";

    /// <summary>
    /// Gets or sets the Clerk test token.
    /// </summary>
    [Required(ErrorMessage = "Clerk TestToken configuration is required")]
    public string TestToken { get; set; } = null!;
}
