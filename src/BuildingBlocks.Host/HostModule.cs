using BuildingBlocks.Host.Endpoints;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Modules;
using System.Reflection;

namespace BuildingBlocks.Host;

public class HostModule : IModule
{
    public string Name => "Host";

    public static class Roles
    {
        public const string Admin = "host:admin";
        public const string User = "host:user";
    }

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddModule(configuration, Name)
            .AddCQRS(Assembly.GetExecutingAssembly())
            .Build();
    }

    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        if (app is WebApplication application)
        {
            application.MapHostEndpoints();
        }
    }

    public Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
