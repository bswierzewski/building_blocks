using Supabase;
using BuildingBlocks.Tests.EndToEnd.Auth;
using BuildingBlocks.Tests.EndToEnd.Options;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Tests.EndToEnd.Auth.Providers;

/// <summary>
/// Fetches JWT tokens from Supabase by authenticating with email/password.
/// Requires SupabaseOptions to be configured with URL, KEY, TEST_EMAIL, and TEST_PASSWORD.
/// </summary>
public class SupabaseAuthTokenProvider : IAuthTokenProvider
{
    private readonly string _url;
    private readonly string _key;
    private readonly string _email;
    private readonly string _password;

    /// <summary>
    /// Initializes a new instance of the SupabaseAuthTokenProvider class.
    /// </summary>
    /// <param name="options">The Supabase authentication options.</param>
    /// <exception cref="InvalidOperationException">Thrown when required Supabase options are not set.</exception>
    public SupabaseAuthTokenProvider(IOptions<SupabaseAuthOptions> options)
    {
        var opts = options.Value;

        _url = opts.Url
            ?? throw new InvalidOperationException("Supabase URL configuration is required");

        _key = opts.Key
            ?? throw new InvalidOperationException("Supabase Key configuration is required");

        _email = opts.TestEmail
            ?? throw new InvalidOperationException("Supabase TestEmail configuration is required");

        _password = opts.TestPassword
            ?? throw new InvalidOperationException("Supabase TestPassword configuration is required");
    }

    /// <summary>
    /// Gets a JWT token from Supabase by authenticating with email and password.
    /// </summary>
    /// <returns>A task that returns the JWT token string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when authentication with Supabase fails.</exception>
    public async Task<string> GetTokenAsync()
    {
        var options = new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };

        var client = new Client(_url, _key, options);
        await client.InitializeAsync();

        var session = await client.Auth.SignIn(_email, _password);

        if (session?.AccessToken == null)
        {
            throw new InvalidOperationException("Failed to authenticate with Supabase. Check credentials.");
        }

        return session.AccessToken;
    }
}
