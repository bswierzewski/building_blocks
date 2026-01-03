using BuildingBlocks.Abstractions.Abstractions;

namespace BuildingBlocks.Tools.Tests.Unit.Options;

/// <summary>
/// Configuration options with a colon in the section name.
/// This tests the replacement of colons with double underscores.
/// </summary>
public class ColonSectionOptions : IOptions
{
    /// <summary>
    /// The configuration section name with colon.
    /// </summary>
    public static string SectionName => "Auth:SectionName";

    public string Audience { get; set; } = "my-app";
    public string ClientId { get; set; } = string.Empty;
    public string Authority { get; set; } = "https://auth.example.com";
}
