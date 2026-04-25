using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Core.Primitives;
using BuildingBlocks.Identity.Infrastructure.Persistence;
using BuildingBlocks.Identity.Infrastructure.Services;
using BuildingBlocks.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Identity;

/// <summary>
/// Registers the Identity module services and applies its database migrations at runtime.
/// </summary>
public sealed class IdentityModule : IModule
{
    public string Name => "Identity";

    public IReadOnlyCollection<Permission> Permissions =>
    [
        new("identity.users.read", "Read identity users"),
        new("identity.users.write", "Manage identity users"),
        new("identity.roles.read", "Read identity roles"),
        new("identity.roles.write", "Manage identity roles")
    ];

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddAuthorization();

        services.AddPostgres<IdentityDbContext>(IdentityDbContext.SchemaName);
    }

    public async Task InitializeMigrationsAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await services.MigrateDatabaseAsync<IdentityDbContext>(cancellationToken);
    }
}