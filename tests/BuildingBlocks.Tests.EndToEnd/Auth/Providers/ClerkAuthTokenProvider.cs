using BuildingBlocks.Tests.EndToEnd.Auth;
using BuildingBlocks.Tests.EndToEnd.Options;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Tests.EndToEnd.Auth.Providers;

/// <summary>
/// Returns a static JWT token from Clerk.
/// Requires ClerkOptions to be configured with TestToken.
/// For Clerk, tokens are typically obtained manually through the dashboard or frontend and provided as static values.
/// </summary>
public class ClerkAuthTokenProvider : IAuthTokenProvider
{
    private readonly string _token;

    /// <summary>
    /// Initializes a new instance of the ClerkAuthTokenProvider class.
    /// </summary>
    /// <param name="options">The Clerk authentication options.</param>
    /// <exception cref="InvalidOperationException">Thrown when required Clerk options are not set.</exception>
    public ClerkAuthTokenProvider(IOptions<ClerkAuthOptions> options)
    {
        var opts = options.Value;

        _token = opts.TestToken
            ?? throw new InvalidOperationException("Clerk TestToken configuration is required");
    }

    /// <summary>
    /// Gets the static JWT token from environment variables.
    /// </summary>
    /// <returns>A task that returns the JWT token string.</returns>
    public Task<string> GetTokenAsync()
    {
        return Task.FromResult(_token);
    }
}
