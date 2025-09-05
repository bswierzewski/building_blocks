using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using BuildingBlocks.Application.Configuration;

namespace BuildingBlocks.Application;

/// <summary>
/// Provides extension methods for configuring application services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds basic application services to the dependency injection container.
    /// This method registers core services but does not configure any modules.
    /// Use AddModule() to configure individual modules with their specific requirements.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddBuildingBlocksApplication(this IServiceCollection services)
    {
        // Register core application services here if needed
        return services;
    }

    /// <summary>
    /// Configures application services for a specific module with granular control over features.
    /// Each module can choose which behaviors and features to enable based on its requirements.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="moduleAssembly">The assembly containing the module's handlers and validators.</param>
    /// <param name="configure">Configuration action for the module builder.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddModule(
        this IServiceCollection services,
        Assembly moduleAssembly,
        Action<ModuleApplicationBuilder> configure)
    {
        var builder = new ModuleApplicationBuilder(services, moduleAssembly);
        configure(builder);
        return services;
    }
}