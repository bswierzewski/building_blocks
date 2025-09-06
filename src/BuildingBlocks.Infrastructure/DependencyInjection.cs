using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Application;

namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Extension methods for unified module registration.
/// Provides a single entry point for configuring both Application and Infrastructure layers.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Registers a complete module with Application and Infrastructure configuration.
    /// Call this BEFORE AddDbContext to register services, interceptors and migrations.
    /// All features are enabled by default, use configuration actions to disable specific ones.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type for this module.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureApplication">Optional configuration action for the application layer (MediatR, validators, behaviors). If null, all Application features are enabled by default.</param>
    /// <param name="configureInfrastructure">Optional configuration action for the infrastructure layer (interceptors, migrations). If null, all Infrastructure features are enabled by default.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddModule<TContext>(
        this IServiceCollection services,
        Action<ModuleApplicationBuilder>? configureApplication = null,
        Action<ModuleInfrastructureBuilder<TContext>>? configureInfrastructure = null)
        where TContext : DbContext
    {
        // Always configure Application layer (with optional customization)
        var applicationBuilder = new ModuleApplicationBuilder(services);
        configureApplication?.Invoke(applicationBuilder);
        applicationBuilder.RegisterServices();

        // Always configure Infrastructure layer (with optional customization)
        var infrastructureBuilder = new ModuleInfrastructureBuilder<TContext>(services);
        configureInfrastructure?.Invoke(infrastructureBuilder);
        infrastructureBuilder.RegisterServices();

        // Register the builder so AddModuleInterceptors can access it
        services.AddSingleton(infrastructureBuilder);

        return services;
    }

    /// <summary>
    /// Extension method to add module interceptors to DbContextOptionsBuilder.
    /// Call this in your DbContext configuration to automatically include enabled interceptors.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="optionsBuilder">The DbContextOptionsBuilder.</param>
    /// <param name="serviceProvider">The service provider to resolve interceptors from.</param>
    /// <returns>The DbContextOptionsBuilder for method chaining.</returns>
    public static DbContextOptionsBuilder<TContext> AddModuleInterceptors<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        var builder = serviceProvider.GetService<ModuleInfrastructureBuilder<TContext>>();
        if (builder != null)
        {
            var interceptors = new List<IInterceptor>();

            // Add interceptors based on enabled features
            foreach (var interceptorType in builder.GetEnabledInterceptorTypes())
            {
                var interceptor = serviceProvider.GetRequiredService(interceptorType);
                if (interceptor is IInterceptor typedInterceptor)
                {
                    interceptors.Add(typedInterceptor);
                }
            }

            if (interceptors.Any())
            {
                optionsBuilder.AddInterceptors(interceptors);
            }
        }
        return optionsBuilder;
    }


}