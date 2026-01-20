using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Kernel.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        public T LoadOptions<T>() where T : class, IOptions, new()
        {
            var options = new T();
            configuration.GetSection(T.SectionName).Bind(options);
            return options;
        }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureOptions<T>(IConfiguration configuration) where T : class, IOptions, new()
        {
            services.Configure<T>(configuration.GetSection(T.SectionName));
            return services;
        }
    }
}
