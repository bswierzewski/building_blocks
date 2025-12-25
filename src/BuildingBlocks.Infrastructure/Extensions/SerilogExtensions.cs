using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class SerilogExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddSerilog()
        {
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services);
            });

            return builder;
        }
    }
}
