using BuildingBlocks.Tests.EndToEnd.Auth;

namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Configuration for selecting the authentication provider in E2E tests.
/// </summary>
public class AuthProviderOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Authentication";

    /// <summary>
    /// Gets or sets the authentication provider type.
    /// Default is None (no authentication).
    /// </summary>
    public AuthProvider Provider { get; set; } = AuthProvider.None;
}
