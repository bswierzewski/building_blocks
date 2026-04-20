using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Modules;
using BuildingBlocks.Infrastructure.Persistence.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Identity;

public sealed class IdentityModule : IModule
{
    public string Name => "Identity";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgres<IdentityDbContext>();
    }

    public async Task InitializeMigrationsAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await services.MigrateDatabaseAsync<IdentityDbContext>(cancellationToken);
    }
}