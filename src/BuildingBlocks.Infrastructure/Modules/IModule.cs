using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Modules;

public interface IModule
{
    string Name { get; }
    void Register(IServiceCollection services, IConfiguration configuration);
    void Use(IApplicationBuilder app, IConfiguration configuration);
    Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}
