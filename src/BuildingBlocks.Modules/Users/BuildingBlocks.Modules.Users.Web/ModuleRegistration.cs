using BuildingBlocks.Modules.Users.Application.Module;
using BuildingBlocks.Modules.Users.Infrastructure.Module;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Modules.Users.Web;

/// <summary>
/// Extension methods for registering the Users module services.
/// </summary>
public static class ModuleRegistration
{
    /// <summary>
    /// Registers all services for the Users module (Application + Infrastructure layers).
    /// </summary>
    public static IServiceCollection AddUsers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }
}
