using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Attributes;

namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Clerk authentication configuration for E2E tests.
/// </summary>
public class ClerkOptions : IOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public static string SectionName => "Clerk";

    /// <summary>
    /// Gets or sets the Clerk test token.
    /// </summary>
    [Required(ErrorMessage = "TestToken is required")]
    [EnvVariable(Description = "Clerk test token for E2E tests", Sensitive = true)]
    public string TestToken { get; set; } = null!;
}
