using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Postgresql;

namespace BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// Provides modular bootstrap extension methods for ASP.NET Core applications
/// using a Wolverine-based module system.
/// </summary>
public static class WebApplicationBootstrapExtensions
{
    /// <summary>
    /// Registers shared modular runtime infrastructure for the provided modules.
    /// </summary>
    public static void AddModuleInfrastructure(
        this WebApplicationBuilder builder,
        IModule[] modules,
        Action<WolverineOptions>? configure = null)
    {
        var dataSource = builder.Services.AddPostgresDataSource(builder.Configuration);

        builder.Host.UseWolverine(opts =>
        {
            // Enable FluentValidation integration so message and HTTP handler validation
            // failures are automatically converted to structured problem details responses.
            opts.UseFluentValidation();

            // Treat each handler type that matches a given message as a separate execution path
            // rather than chaining them into a single pipeline. Prevents unintentional fan-out.
            opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

            opts.PersistMessagesWithPostgresql(dataSource, "wolverine");

            // Enlist Wolverine in EF Core transactions so that message dispatch and database
            // writes participate in the same unit of work and commit atomically.
            opts.UseEntityFrameworkCoreTransactions();

            // Automatically wrap every handler that opens a DbContext in the transactional
            // outbox policy without requiring per-handler opt-in attributes.
            opts.Policies.AutoApplyTransactions();

            // Always scan the BuildingBlocks.Infrastructure assembly so that shared middleware
            // (e.g. LoggingMiddleware) is discovered regardless of which module assemblies are loaded.
            opts.Discovery.IncludeAssembly(typeof(LoggingMiddleware).Assembly);

            // Scan each module assembly so Wolverine discovers its HTTP endpoints,
            // message handlers, and any module-specific middleware or policies.
            foreach (var module in modules)
                opts.Discovery.IncludeAssembly(module.GetType().Assembly);

            // Allow the caller to extend or override Wolverine options without modifying
            // this shared helper — useful for application-specific transports or policies.
            configure?.Invoke(opts);
        });

        // Register the ASP.NET Core bridge that maps Wolverine HTTP endpoints into the
        // routing pipeline. Must be called before MapModuleEndpoints on the built app.
        builder.Services.AddWolverineHttp();
    }

    /// <summary>
    /// Maps Wolverine HTTP endpoints and enables FluentValidation problem details middleware.
    /// </summary>
    public static void MapModuleEndpoints(this WebApplication app)
    {
        app.MapWolverineEndpoints(opts =>
        {
            // Add middleware that intercepts FluentValidation failures thrown by Wolverine
            // HTTP handlers and converts them to RFC 7807 ValidationProblemDetails responses,
            // keeping the error format consistent across the entire API surface.
            opts.UseFluentValidationProblemDetailMiddleware();
        });
    }
}
