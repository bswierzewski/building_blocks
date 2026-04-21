using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Identity.Configuration;

public sealed class ClerkAuthenticationOptions
{
    public const string SectionName = "Identity:Clerk";

    [Required]
    [Url]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    public string AuthorizedParty { get; init; } = string.Empty;
}