using BuildingBlocks.Tests.EndToEnd.Auth.Providers;
using BuildingBlocks.Tests.EndToEnd.Options;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Tests.EndToEnd.Auth;

/// <summary>
/// Factory for creating appropriate auth token provider based on configuration.
/// Uses IOptions&lt;AuthOptions&gt; to determine which provider to use.
/// Supported values: "supabase", "clerk"
/// </summary>
public static class AuthTokenProviderFactory
{
    /// <summary>
    /// Creates an appropriate auth token provider based on the AuthOptions configuration.
    /// </summary>
    /// <param name="options">The authentication options.</param>
    /// <returns>An IAuthTokenProvider implementation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when provider is not set or has an unsupported value.</exception>
    public static IAuthTokenProvider Create(IOptions<AuthOptions> options)
    {
        var authOptions = options.Value;

        if (!Enum.TryParse<AuthProvider>(authOptions.Provider, ignoreCase: true, out var provider))
        {
            throw new InvalidOperationException(
                $"Unknown Auth:Provider: {authOptions.Provider}. Supported values: 'supabase', 'clerk'");
        }

        return provider switch
        {
            AuthProvider.Supabase => new SupabaseAuthTokenProvider(Microsoft.Extensions.Options.Options.Create(authOptions.Supabase)),
            AuthProvider.Clerk => new ClerkAuthTokenProvider(Microsoft.Extensions.Options.Options.Create(authOptions.Clerk)),
            _ => throw new InvalidOperationException($"Unknown provider: {provider}")
        };
    }
}
