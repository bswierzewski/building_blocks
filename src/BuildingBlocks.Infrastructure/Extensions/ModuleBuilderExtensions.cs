using BuildingBlocks.Infrastructure.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class ModuleBuilderExtensions
{
    extension(IServiceCollection services)
    {
        public ModuleBuilder AddModule(IConfiguration configuration, string moduleName)
        {
            return new ModuleBuilder(services, configuration, moduleName);
        }

        public IServiceCollection RegisterModules(IConfiguration configuration, params IModule[] modules)
        {
            foreach (var module in modules)
            {
                module.Register(services, configuration);
                services.AddSingleton(module);
            }

            return services;
        }
    }

    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseModules(IConfiguration configuration)
        {
            var modules = app.ApplicationServices.GetServices<IModule>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<IModule>>();

            foreach (var module in modules)
            {
                try
                {
                    logger.LogInformation("Configuring module '{ModuleName}'...", module.Name);
                    module.Use(app, configuration);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error configuring module '{ModuleName}'", module.Name);
                    throw;
                }
            }

            return app;
        }
    }

    extension(IServiceProvider serviceProvider)
    {
        public async Task InitModules(CancellationToken cancellationToken = default)
        {
            var modules = serviceProvider.GetServices<IModule>();
            var logger = serviceProvider.GetRequiredService<ILogger<IModule>>();

            foreach (var module in modules)
            {
                try
                {
                    logger.LogInformation("Initializing module '{ModuleName}'...", module.Name);
                    await module.Initialize(serviceProvider, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error initializing module '{ModuleName}'", module.Name);
                    throw;
                }
            }
        }
    }
}
