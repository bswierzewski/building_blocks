using BuildingBlocks.Modules.Users.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Modules.Users.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring authentication options.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Configures Clerk authentication options from configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddClerkOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ClerkOptions>(configuration.GetSection(ClerkOptions.SectionName));

        // Add validation
        services.AddOptions<ClerkOptions>()
            .Bind(configuration.GetSection(ClerkOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Configures Auth0 authentication options from configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAuth0Options(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Auth0Options>(configuration.GetSection(Auth0Options.SectionName));

        // Add validation
        services.AddOptions<Auth0Options>()
            .Bind(configuration.GetSection(Auth0Options.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
