using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Infrastructure.Configuration;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;

namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Provides extension methods for dependency injection configuration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers shared infrastructure services.
    /// This method registers core infrastructure services like TimeProvider.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddBuildingBlocksInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        return services;
    }

    /// <summary>
    /// Configures infrastructure services for a specific DbContext module with granular control over features.
    /// Each module can choose which infrastructure features to enable based on its requirements.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type for this module.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">Configuration action for the module infrastructure builder.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddModuleInfrastructure<TContext>(
        this IServiceCollection services,
        Action<ModuleInfrastructureBuilder<TContext>> configure)
        where TContext : DbContext
    {
        var builder = new ModuleInfrastructureBuilder<TContext>(services);
        configure(builder);
        return services;
    }
}