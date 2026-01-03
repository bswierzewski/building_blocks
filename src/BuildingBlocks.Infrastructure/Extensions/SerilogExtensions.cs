using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class SerilogExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddSerilog(string? applicationName = null)
        {
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                var isDevelopment = context.HostingEnvironment.IsDevelopment();
                var appName = applicationName ?? context.HostingEnvironment.ApplicationName;

                // Configure defaults first
                configuration
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithSpan()
                    .Enrich.WithProperty("Application", appName);

                // Set minimum levels based on environment
                if (isDevelopment)
                {
                    configuration
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                        // Keep EF Core at Information in Dev to see SQL queries (optional)
                        // .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information) 
                        .MinimumLevel.Override("System", LogEventLevel.Information);
                }
                else
                {
                    configuration
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Silence framework noise
                        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        // Exception: We want to know when the app starts/stops
                        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
                }

                configuration.WriteTo.Async(writeTo =>
                {

                    var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [{TraceId}] {Message:lj}{NewLine}{Exception}";

                    writeTo.File(
                        path: "Logs/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: outputTemplate);

                    // DEVELOPMENT: Human-readable text
                    if (isDevelopment)
                    {
                        writeTo.Console(outputTemplate: outputTemplate);
                    }
                    // PRODUCTION: Machine-readable JSON (Compact)
                    else
                    {
                        // Use CompactJsonFormatter for Docker/Kubernetes/ELK Stack
                        // TraceId is automatically included in the JSON structure
                        writeTo.Console(new CompactJsonFormatter());
                    }
                });

                // Finally, allow appsettings.json to override any of the above
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            return builder;
        }
    }
}
