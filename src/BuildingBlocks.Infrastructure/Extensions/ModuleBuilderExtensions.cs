using BuildingBlocks.Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class ModuleBuilderExtensions
{
    extension(IServiceCollection services)
    {
        public ModuleBuilder AddModule(IConfiguration configuration, string moduleName)
        {
            return new ModuleBuilder(services, configuration, moduleName);
        }
    }
}
