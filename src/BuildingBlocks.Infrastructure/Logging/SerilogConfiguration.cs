using Microsoft.Extensions.Hosting;
using Serilog;

namespace BuildingBlocks.Infrastructure.Logging;

public static class SerilogConfiguration
{
    extension(IHostBuilder host)
    {
        public IHostBuilder AddSerilog()
        {
            host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services);
            });

            return host;
        }
    }
}
