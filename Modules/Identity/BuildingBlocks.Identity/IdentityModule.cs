using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Identity.Configuration;
using BuildingBlocks.Infrastructure.Modules.Extensions;
using BuildingBlocks.Identity.Infrastructure.Authentication.Services;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Persistence.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Identity;

/// <summary>
/// Registers the Identity module services and applies its database migrations at runtime.
/// </summary>
public sealed class IdentityModule : IModule
{
    public string Name => "Identity";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<JitProvisioningOptions>(configuration, JitProvisioningOptions.SectionName);

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<JitProvisioningService>();

        services.AddHttpContextAccessor();
        services.AddAuthorization();

        services.AddPostgres<IdentityDbContext>(IdentityDbContext.SchemaName);
    }

    public async Task InitializeMigrationsAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await services.MigrateDatabaseAsync<IdentityDbContext>(cancellationToken);
    }
}