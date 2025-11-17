using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Attributes;
using BuildingBlocks.Tests.EndToEnd.Auth;

namespace BuildingBlocks.Tests.EndToEnd.Options;

/// <summary>
/// Configuration for selecting the authentication provider in E2E tests.
/// </summary>
public class AuthProviderOptions : IOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public static string SectionName => "Authentication";

    /// <summary>
    /// Gets or sets the authentication provider type.
    /// Default is None (no authentication).
    /// Valid values: None, Supabase, Auth0, Clerk
    /// </summary>
    [EnvVariable(Description = "Authentication provider (None, Supabase, Auth0, Clerk)", DefaultValue = "None")]
    public AuthProvider Provider { get; set; } = AuthProvider.None;
}
