using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Tests.EndToEnd.Options;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Tests.EndToEnd.Auth.Providers;

/// <summary>
/// Returns a static JWT token from Clerk.
/// Configuration is loaded lazily from IConfiguration only when this provider is used.
/// For Clerk, tokens are typically obtained manually through the dashboard or frontend and provided as static values.
/// Token is cached internally after first fetch.
/// </summary>
public class ClerkAuthTokenProvider : IAuthTokenProvider
{
    private readonly string _token;

    /// <summary>
    /// Initializes a new instance of the ClerkAuthTokenProvider class.
    /// Loads configuration from IConfiguration and validates it.
    /// Token is cached during initialization.
    /// </summary>
    /// <param name="configuration">The configuration to load Clerk settings from.</param>
    /// <exception cref="ValidationException">Thrown when required configuration is missing.</exception>
    public ClerkAuthTokenProvider(IConfiguration configuration)
    {
        var options = new ClerkAuthOptions();
        configuration.GetSection(ClerkAuthOptions.SectionName)
            .Bind(options);

        // Validate configuration using DataAnnotations
        var validationContext = new ValidationContext(options);
        Validator.ValidateObject(options, validationContext, validateAllProperties: true);

        // Cache token during initialization
        _token = options.TestToken;
    }

    /// <summary>
    /// Gets the static JWT token from configuration.
    /// Token is cached and returned immediately.
    /// </summary>
    /// <returns>A task that returns the JWT token string.</returns>
    public Task<string?> GetTokenAsync()
    {
        return Task.FromResult<string?>(_token);
    }
}
