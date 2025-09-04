using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using BuildingBlocks.Application.Behaviors;

namespace BuildingBlocks.Application;

/// <summary>
/// Provides extension methods for configuring application services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });
    }
}