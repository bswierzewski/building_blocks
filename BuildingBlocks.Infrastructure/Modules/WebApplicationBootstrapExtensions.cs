using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Infrastructure.Middleware;
using BuildingBlocks.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    /// Registers module services, NpgsqlDataSource, fully-configured Wolverine (with PostgreSQL
    /// persistence and EF Core transactions), and a hosted service that runs module initialization.
    /// Use this path for normal application startup.
    /// </summary>
    public static void AddModularRuntimeInfrastructure(
        this WebApplicationBuilder builder,
        IModule[] modules,
        Action<WolverineOptions>? configure = null)
    {
        // Let every module register its own services, DbContexts, and options into the DI container.
        foreach (var module in modules)
            module.AddServices(builder.Services, builder.Configuration);

        // Build a single shared NpgsqlDataSource used by all module DbContexts and Wolverine transport.
        // Creating it here (before UseWolverine) allows passing the instance directly to
        // PersistMessagesWithPostgresql, which requires a concrete NpgsqlDataSource — not a delegate.
        var dataSource = builder.Services.AddNpgsqlDataSource(builder.Configuration);

        builder.Host.UseWolverine(opts =>
        {
            // Enable FluentValidation integration so message and HTTP handler validation
            // failures are automatically converted to structured problem details responses.
            opts.UseFluentValidation();

            // Treat each handler type that matches a given message as a separate execution path
            // rather than chaining them into a single pipeline. Prevents unintentional fan-out.
            opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

            // Persist Wolverine inbox/outbox and durable messaging state in PostgreSQL under
            // the "wolverine" schema, sharing the same data source as all module DbContexts.
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
        // routing pipeline. Must be called before MapModularEndpoints on the built app.
        builder.Services.AddWolverineHttp();

        // Register the module array as a singleton so ModuleInitializerService can resolve
        // it via constructor injection and iterate over every module during startup.
        builder.Services.AddSingleton<IModule[]>(modules);

        // Register the hosted service that drives post-startup module initialization
        // (e.g. applying EF migrations and seeding reference data).
        builder.Services.AddHostedService<ModuleInitializerService>();
    }

    /// <summary>
    /// Registers module services and Wolverine handler/endpoint discovery only — no database
    /// connection, no persistence, no hosted initialization. Suitable for OpenAPI document
    /// generation (<c>ASPNETCORE_ENVIRONMENT=Tooling</c>) where side effects must be prevented.
    /// </summary>
    public static void AddModularToolingInfrastructure(
        this WebApplicationBuilder builder,
        IModule[] modules,
        Action<WolverineOptions>? configure = null)
    {
        // Register module services so their DI dependencies and OpenAPI metadata are available
        // to the document generator. No NpgsqlDataSource is built — there is no database connection.
        foreach (var module in modules)
            module.AddServices(builder.Services, builder.Configuration);

        builder.Host.UseWolverine(opts =>
        {
            // Enable FluentValidation so generated request/response schemas include validation
            // annotations that would be present at runtime.
            opts.UseFluentValidation();

            // Mirror the runtime handler behavior setting so the generated spec reflects
            // the actual routing semantics of the live application.
            opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

            // Scan BuildingBlocks so shared endpoint conventions are included in the spec
            // even when no module-level middleware is registered.
            opts.Discovery.IncludeAssembly(typeof(LoggingMiddleware).Assembly);

            // Scan every module assembly so all HTTP endpoints appear in the generated document.
            foreach (var module in modules)
                opts.Discovery.IncludeAssembly(module.GetType().Assembly);

            configure?.Invoke(opts);
        });

        // Register the HTTP bridge so MapModularEndpoints can map discovered endpoints
        // into the routing pipeline for the document generator to introspect.
        builder.Services.AddWolverineHttp();
    }

    /// <summary>
    /// Maps Wolverine HTTP endpoints and enables FluentValidation problem details middleware.
    /// </summary>
    public static void MapModularEndpoints(this WebApplication app)
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
