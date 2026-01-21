using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.OpenTelemetry;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog with Aspire and Wolverine telemetry integration.
    /// Settings can be overridden in appsettings.json or docker-compose.
    /// </summary>
    public static IHostBuilder UseSerilog(
        this IHostBuilder hostBuilder,
        Action<SerilogBuilder> configure)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfig) =>
        {
            var builder = new SerilogBuilder(context.Configuration, context.HostingEnvironment);

            // Apply defaults that work with Aspire and Wolverine
            loggerConfig
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithSpan()
                .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

            // Set default minimum levels based on environment
            if (context.HostingEnvironment.IsDevelopment())
            {
                loggerConfig
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Information);
            }
            else
            {
                loggerConfig
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
            }

            // Apply user configuration (sinks)
            configure(builder);

            // Apply all sink configurations
            foreach (var action in builder.GetConfigurationActions())            
                action(loggerConfig);

            // Allow appsettings.json to override everything
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });
    }
}

public sealed class SerilogBuilder
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly List<Action<LoggerConfiguration>> _configurationActions = new();

    internal SerilogBuilder(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    internal IEnumerable<Action<LoggerConfiguration>> GetConfigurationActions() => _configurationActions;

    /// <summary>
    /// Adds console sink. In Development uses simplified text format, in Production uses compact JSON.
    /// All settings can be overridden in appsettings.json.
    /// </summary>
    public SerilogBuilder AddConsole()
    {
        var isDevelopment = _environment.IsDevelopment();

        _configurationActions.Add(config =>
        {
            config.WriteTo.Async(writeTo =>
            {
                if (isDevelopment)
                {
                    writeTo.Console(outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                        "[{TraceId}] " +
                        "{SourceContext} " +
                        "{Message:lj}{NewLine}{Exception}");
                }
                else
                {
                    writeTo.Console(new CompactJsonFormatter());
                }
            });
        });

        return this;
    }

    /// <summary>
    /// Adds file sink with daily rolling logs.
    /// All settings can be overridden in appsettings.json.
    /// </summary>
    public SerilogBuilder AddFile()
    {
        _configurationActions.Add(config =>
        {
            config.WriteTo.Async(writeTo =>
            {
                writeTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                        "[{TraceId}] " +
                        "{SourceContext} " +
                        "{Message:lj}{NewLine}{Exception}");
            });
        });

        return this;
    }

    /// <summary>
    /// Adds OpenTelemetry sink for Aspire Dashboard integration.
    /// Automatically detects OTLP endpoint from environment variables set by Aspire.
    /// All settings can be overridden in appsettings.json.
    /// </summary>
    public SerilogBuilder AddOpenTelemetry()
    {
        var otlpEndpoint = _configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var serviceName = _configuration["OTEL_SERVICE_NAME"] ?? _environment.ApplicationName;
        var environmentName = _environment.EnvironmentName;

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            _configurationActions.Add(config =>
            {
                config.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.Protocol = OtlpProtocol.Grpc;

                    options.IncludedData = IncludedData.TraceIdField
                        | IncludedData.SpanIdField
                        | IncludedData.SourceContextAttribute
                        | IncludedData.MessageTemplateTextAttribute
                        | IncludedData.MessageTemplateMD5HashAttribute;

                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName,
                        ["deployment.environment"] = environmentName
                    };
                });
            });
        }

        return this;
    }
}
