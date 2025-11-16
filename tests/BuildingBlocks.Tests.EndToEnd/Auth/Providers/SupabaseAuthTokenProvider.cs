using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Tests.EndToEnd.Options;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Tests.EndToEnd.Auth.Providers;

/// <summary>
/// Fetches JWT tokens from Supabase by authenticating with email/password.
/// Configuration is loaded lazily from IConfiguration only when this provider is used.
/// Token is cached internally after first fetch.
/// </summary>
public class SupabaseAuthTokenProvider : IAuthTokenProvider
{
    private readonly SupabaseOptions _options;
    private string? _cachedToken;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the SupabaseAuthTokenProvider class.
    /// Loads configuration from IConfiguration and validates it.
    /// </summary>
    /// <param name="configuration">The configuration to load Supabase settings from.</param>
    /// <exception cref="ValidationException">Thrown when required configuration is missing.</exception>
    public SupabaseAuthTokenProvider(IConfiguration configuration)
    {
        _options = new SupabaseOptions();
        configuration.GetSection(SupabaseOptions.SectionName).Bind(_options);

        // Validate configuration using DataAnnotations
        var validationContext = new ValidationContext(_options);
        Validator.ValidateObject(_options, validationContext, validateAllProperties: true);
    }

    /// <summary>
    /// Gets a JWT token from Supabase by authenticating with email and password.
    /// Token is cached after first fetch and reused for subsequent calls.
    /// </summary>
    /// <returns>A task that returns the JWT token string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when authentication with Supabase fails.</exception>
    public async Task<string?> GetTokenAsync()
    {
        // Return cached token if available
        if (_cachedToken != null)
            return _cachedToken;

        // Use semaphore to ensure only one authentication happens
        await _lock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cachedToken != null)
                return _cachedToken;

            var supabaseOptions = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = false,
                AutoConnectRealtime = false
            };

            var client = new Supabase.Client(_options.Url, _options.Key, supabaseOptions);
            await client.InitializeAsync();

            var session = await client.Auth.SignIn(_options.TestEmail, _options.TestPassword);

            if (session?.AccessToken == null)
                throw new InvalidOperationException("Failed to authenticate with Supabase. Check credentials.");

            _cachedToken = session.AccessToken;
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}
