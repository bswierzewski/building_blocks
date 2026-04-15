using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Infrastructure.Authentication.Clerk;

/// <summary>
/// Clerk-specific authentication settings used by the API and test hosts.
/// Supports remote Clerk metadata and a local symmetric signing key for tests.
/// </summary>
public sealed class ClerkAuthenticationOptions : IValidatableObject
{
    public const string SectionPath = "Authentication:Clerk";

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    public string? Authority { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Authority))
        {
            yield return new ValidationResult(
                $"{nameof(Authority)} must be configured.",
                [nameof(Authority)]);
        }
    }
}