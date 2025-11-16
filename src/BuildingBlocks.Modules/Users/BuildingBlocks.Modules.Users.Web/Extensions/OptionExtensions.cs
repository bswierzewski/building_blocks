using BuildingBlocks.Modules.Users.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Modules.Users.Web.Extensions;

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
        services.AddOptions<Auth0Options>()
            .Bind(configuration.GetSection(Auth0Options.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Configures Supabase authentication options from configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSupabaseOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SupabaseOptions>()
            .Bind(configuration.GetSection(SupabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
