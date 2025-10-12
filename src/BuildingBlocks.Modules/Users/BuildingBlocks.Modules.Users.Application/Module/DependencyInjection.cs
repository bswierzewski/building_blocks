using System.Reflection;
using BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Modules.Users.Application.Module;

/// <summary>
/// Dependency injection configuration for the Users Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all application layer services for the Users module.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidators();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddLoggingBehavior()
               .AddUnhandledExceptionBehavior()
               .AddValidationBehavior()
               .AddAuthorizationBehavior()
               .AddPerformanceMonitoringBehavior();
        });

        return services;
    }
}
