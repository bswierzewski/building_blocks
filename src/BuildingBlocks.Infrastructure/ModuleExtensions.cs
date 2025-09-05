using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using BuildingBlocks.Application.Configuration;
using BuildingBlocks.Infrastructure.Configuration;

namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Unified module builder that configures both Application and Infrastructure layers.
/// Provides a single entry point for module configuration.
/// </summary>
/// <typeparam name="TContext">The DbContext type for this module.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ModuleBuilder{TContext}"/> class.
/// Each builder will automatically detect the appropriate calling assembly.
/// </remarks>
/// <param name="services">The service collection.</param>
internal class ModuleBuilder<TContext>(IServiceCollection services) where TContext : DbContext
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Gets the application layer builder for this module.
    /// </summary>
    public ModuleApplicationBuilder Application { get; } = new ModuleApplicationBuilder(services);

    /// <summary>
    /// Gets the infrastructure layer builder for this module.
    /// </summary>
    public ModuleInfrastructureBuilder<TContext> Infrastructure { get; } = new ModuleInfrastructureBuilder<TContext>(services);

    /// <summary>
    /// Configures the module with specified Application and Infrastructure settings.
    /// If no configuration is provided, defaults to full module setup (all features enabled).
    /// </summary>
    /// <param name="configureApplication">Optional configuration action for the application layer. If null, enables all features.</param>
    /// <param name="configureInfrastructure">Optional configuration action for the infrastructure layer. If null, enables all features.</param>
    public void Configure(
        Action<ModuleApplicationBuilder>? configureApplication,
        Action<ModuleInfrastructureBuilder<TContext>>? configureInfrastructure)
    {
        // Configure Application layer (apply configuration or use defaults)
        configureApplication?.Invoke(Application);
        Application.RegisterServices();

        // Configure Infrastructure layer (apply configuration or use defaults)
        configureInfrastructure?.Invoke(Infrastructure);
        Infrastructure.RegisterServices();
    }
}

/// <summary>
/// Extension methods for unified module registration.
/// Provides a single entry point for configuring both Application and Infrastructure layers.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Registers a complete module with both Application and Infrastructure configuration.
    /// Automatically detects the calling assembly containing the module's handlers, validators, and DbContext.
    /// If no configuration is provided, defaults to full module setup with all features enabled.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type for this module.</typeparam>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureApplication">Optional configuration action for the application layer. If null, adds full module (handlers, validators, all behaviors).</param>
    /// <param name="configureInfrastructure">Optional configuration action for the infrastructure layer. If null, adds full module (migrations, all interceptors).</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// // Full module (default - everything enabled)
    /// services.AddModule&lt;OrdersDbContext&gt;();
    /// 
    /// // Custom application, full infrastructure
    /// services.AddModule&lt;OrdersDbContext&gt;(
    ///     configureApplication: app =&gt; app.AddHandlers().AddValidation());
    /// 
    /// // Full application, custom infrastructure
    /// services.AddModule&lt;OrdersDbContext&gt;(
    ///     configureInfrastructure: infra =&gt; infra.AddAuditableInterceptor().AddMigrations());
    /// 
    /// // Custom both layers
    /// services.AddModule&lt;OrdersDbContext&gt;(
    ///     configureApplication: app =&gt; app.AddHandlers().AddLogging(),
    ///     configureInfrastructure: infra =&gt; infra.AddDomainEventDispatch());
    /// 
    /// // Read-only module
    /// services.AddModule&lt;ReportsDbContext&gt;(
    ///     configureApplication: app =&gt; app.AddHandlers().AddLogging(),
    ///     configureInfrastructure: _ =&gt; { }); // No infrastructure services
    /// </example>
    public static IServiceCollection AddModule<TContext>(
        this IServiceCollection services,
        Action<ModuleApplicationBuilder>? configureApplication = null,
        Action<ModuleInfrastructureBuilder<TContext>>? configureInfrastructure = null)
        where TContext : DbContext
    {
        var builder = new ModuleBuilder<TContext>(services);
        builder.Configure(configureApplication, configureInfrastructure);
        return services;
    }
}