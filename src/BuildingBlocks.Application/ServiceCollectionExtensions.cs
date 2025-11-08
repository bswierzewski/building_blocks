using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using BuildingBlocks.Application.Behaviors;

namespace BuildingBlocks.Application;

/// <summary>
/// Extension methods for IServiceCollection to add application layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds FluentValidation validators from the executing assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        return services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Adds FluentValidation validators from the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for validators.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddValidators(this IServiceCollection services, Assembly assembly)
    {
        return services.AddValidatorsFromAssembly(assembly);
    }
}

/// <summary>
/// Extension methods for MediatRServiceConfiguration to add behaviors and handlers.
/// </summary>
public static class MediatRServiceConfigurationExtensions
{
    /// <summary>
    /// Registers MediatR request handlers from the executing assembly.
    /// </summary>
    /// <param name="configuration">The MediatR service configuration.</param>
    /// <returns>The MediatR service configuration for method chaining.</returns>
    public static MediatRServiceConfiguration RegisterHandlers(this MediatRServiceConfiguration configuration)
    {
        return configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Registers MediatR request handlers from the specified assembly.
    /// </summary>
    /// <param name="configuration">The MediatR service configuration.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The MediatR service configuration for method chaining.</returns>
    public static MediatRServiceConfiguration RegisterHandlers(this MediatRServiceConfiguration configuration, Assembly assembly)
    {
        return configuration.RegisterServicesFromAssembly(assembly);
    }

    /// <summary>
    /// Adds logging behavior to the MediatR pipeline.
    /// Logs request start, completion, and performance metrics.
    /// </summary>
    /// <param name="configuration">The MediatR service configuration.</param>
    /// <returns>The MediatR service configuration for method chaining.</returns>
    public static MediatRServiceConfiguration AddLoggingBehavior(this MediatRServiceConfiguration configuration)
    {
        return configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
    }

    /// <summary>
    /// Adds unhandled exception behavior to the MediatR pipeline.
    /// Catches and logs unhandled exceptions from request handlers.
    /// </summary>
    /// <param name="configuration">The MediatR service configuration.</param>
    /// <returns>The MediatR service configuration for method chaining.</returns>
    public static MediatRServiceConfiguration AddUnhandledExceptionBehavior(this MediatRServiceConfiguration configuration)
    {
        return configuration.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
    }

    /// <summary>
    /// Adds authorization behavior to the MediatR pipeline.
    /// Enforces security policies and permission checks for requests.
    /// </summary>
    /// <param name="configuration">The MediatR service configuration.</param>
    /// <returns>The MediatR service configuration for method chaining.</returns>
    public static MediatRServiceConfiguration AddAuthorizationBehavior(this MediatRServiceConfiguration configuration)
    {
        return configuration.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
    }

    /// <summary>
    /// Adds validation behavior to the MediatR pipeline.
    /// Validates request data using FluentValidation before processing.
    /// </summary>
    /// <param name="configuration">The MediatR service configuration.</param>
    /// <returns>The MediatR service configuration for method chaining.</returns>
    public static MediatRServiceConfiguration AddValidationBehavior(this MediatRServiceConfiguration configuration)
    {
        return configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
    }

    /// <summary>
    /// Adds performance monitoring behavior to the MediatR pipeline.
    /// Measures execution time and logs requests that exceed configured thresholds.
    /// </summary>
    /// <param name="configuration">The MediatR service configuration.</param>
    /// <returns>The MediatR service configuration for method chaining.</returns>
    public static MediatRServiceConfiguration AddPerformanceMonitoringBehavior(this MediatRServiceConfiguration configuration)
    {
        return configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    }

    /// <summary>
    /// Registers MediatR request handlers from the BuildingBlocks.Application assembly.
    /// This includes shared handlers like GetListEnumValuesHandler.
    /// </summary>
    /// <param name="configuration">The MediatR service configuration.</param>
    /// <returns>The MediatR service configuration for method chaining.</returns>
    public static MediatRServiceConfiguration AddBuildingBlocksHandlers(this MediatRServiceConfiguration configuration)
    {
        return configuration.RegisterServicesFromAssembly(typeof(MediatRServiceConfigurationExtensions).Assembly);
    }
}