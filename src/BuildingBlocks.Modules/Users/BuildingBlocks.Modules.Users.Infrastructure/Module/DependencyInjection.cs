using System.Reflection;
using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Infrastructure.Options;
using BuildingBlocks.Modules.Users.Infrastructure.Persistence;
using BuildingBlocks.Modules.Users.Infrastructure.Services;
using BuildingBlocks.Modules.Users.Infrastructure.HostedServices;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Modules.Users.Infrastructure.Module;

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

        // Register database options from configuration
        services.Configure<UsersDatabaseOptions>(
            configuration.GetSection(UsersDatabaseOptions.SectionName));

        // Register EF Core interceptors
        services
            .AddMigrationService<UsersDbContext>()
            .AddAuditableEntityInterceptor()
            .AddDomainEventDispatchInterceptor();

        // Register DbContext with PostgreSQL
        services.AddDbContext<UsersDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<UsersDatabaseOptions>>().Value;

            options.UseNpgsql(dbOptions.ConnectionString)
                   .AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.ISaveChangesInterceptor>());
        });

        // Register DbContext interfaces
        services.AddScoped<IUsersWriteDbContext>(provider => provider.GetRequiredService<UsersDbContext>());
        services.AddScoped<IUsersReadDbContext>(provider => provider.GetRequiredService<UsersDbContext>());

        // Register UserService as IUser implementation (replaces StaticUserService)
        services.AddScoped<IUser, UserService>();

        // Register UserProvisioningService
        services.AddScoped<IUserProvisioningService, UserProvisioningService>();

        // Register Users Module as IModule
        services.AddSingleton<IModule, Module>();

        // Register RolesAndPermissionsHostedService
        services.AddHostedService<RolesAndPermissionsHostedService>();

        return services;
    }
}
