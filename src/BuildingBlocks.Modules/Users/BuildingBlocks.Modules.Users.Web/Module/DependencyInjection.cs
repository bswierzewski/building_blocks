using System.Reflection;
using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Infrastructure.Persistence;
using BuildingBlocks.Modules.Users.Infrastructure.Services;
using BuildingBlocks.Modules.Users.Infrastructure.HostedServices;
using BuildingBlocks.Modules.Users.Infrastructure.Options;
using BuildingBlocks.Modules.Users.Infrastructure.Module;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Modules.Users.Web.Module;

/// <summary>
/// Dependency injection configuration for the Users Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all infrastructure layer services for the Users module.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register EF Core interceptors
        services
            .AddMigrationService<UsersDbContext>()
            .AddAuditableEntityInterceptor()
            .AddDomainEventDispatchInterceptor();

        // Register DbContext with PostgreSQL
        services.AddDbContext<UsersDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("UsersConnection"))
                   .AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.ISaveChangesInterceptor>());
        });

        // Register DbContext interfaces
        services.AddScoped<IUsersWriteDbContext>(provider => provider.GetRequiredService<UsersDbContext>());
        services.AddScoped<IUsersReadDbContext>(provider => provider.GetRequiredService<UsersDbContext>());

        // Register UserService as IUser implementation (replaces StaticUserService)
        services.AddScoped<IUser, UserService>();

        // Register UserProvisioningService
        services.AddScoped<IUserProvisioningService, UserProvisioningService>();

        // Register UsersModule as IModule
        services.AddSingleton<IModule, UsersModule>();

        // Register RolesAndPermissionsHostedService
        services.AddHostedService<RolesAndPermissionsHostedService>();

        return services;
    }

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
