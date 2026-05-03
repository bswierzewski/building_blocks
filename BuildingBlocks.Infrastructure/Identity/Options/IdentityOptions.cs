namespace BuildingBlocks.Infrastructure.Identity.Options;

/// <summary>
/// Configures JWT bearer authentication for the API.
/// Any supplied validation input is applied; omitted values leave that validation path disabled.
/// </summary>
public sealed class IdentityOptions
{
    public const string SectionName = "Authentication:Identity";

    public string? Authority { get; set; }

    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    public string? SigningKey { get; set; }
}