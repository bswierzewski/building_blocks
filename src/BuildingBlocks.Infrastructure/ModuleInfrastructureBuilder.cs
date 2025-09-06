using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Infrastructure.Persistence.Migrations;

namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Builder for configuring infrastructure services for a specific DbContext module.
/// Uses opt-out approach - all features are enabled by default, use DisableX methods to turn off specific features.
/// </summary>
/// <typeparam name="TContext">The DbContext type for this module.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ModuleInfrastructureBuilder{TContext}"/> class.
/// </remarks>
/// <param name="services">The service collection.</param>
public class ModuleInfrastructureBuilder<TContext>(IServiceCollection services) where TContext : DbContext
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    public IServiceCollection Services { get; } = services;

    // Internal flags to track what should be disabled
    private readonly HashSet<string> _disabledFeatures = new();
    
    // Custom interceptors registered for this module
    private readonly List<Type> _customInterceptors = new();

    /// <summary>
    /// Disables automatic database migrations that run on application startup.
    /// By default, the migration service checks for pending migrations and applies them automatically.
    /// WARNING: Only disable if you manage migrations manually or use alternative migration strategies.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleInfrastructureBuilder<TContext> DisableMigrations()
    {
        _disabledFeatures.Add("Migrations");
        return this;
    }

    /// <summary>
    /// Disables automatic audit field population for entities implementing IAuditable.
    /// By default, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy fields are set automatically on SaveChanges.
    /// WARNING: Only disable if you have alternative auditing mechanisms or don't need audit trails.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleInfrastructureBuilder<TContext> DisableAuditableInterceptor()
    {
        _disabledFeatures.Add("AuditableInterceptor");
        return this;
    }

    /// <summary>
    /// Disables automatic domain event dispatching after SaveChanges.
    /// By default, domain events from aggregate roots are published via MediatR after successful save.
    /// WARNING: Only disable if you don't use domain events or have alternative event publishing mechanisms.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public ModuleInfrastructureBuilder<TContext> DisableDomainEventDispatch()
    {
        _disabledFeatures.Add("DomainEventDispatch");
        return this;
    }

    /// <summary>
    /// Internal method to register all enabled services.
    /// Called automatically by the module registration process.
    /// </summary>
    internal void RegisterServices()
    {
        // Register migrations (enabled by default)
        if (!_disabledFeatures.Contains("Migrations"))
            Services.AddHostedService<MigrationService<TContext>>();

        // Register auditable interceptor (enabled by default)
        if (!_disabledFeatures.Contains("AuditableInterceptor"))
            Services.AddScoped<AuditableEntityInterceptor>();

        // Register domain event dispatch interceptor (enabled by default)
        if (!_disabledFeatures.Contains("DomainEventDispatch"))
            Services.AddScoped<DispatchDomainEventsInterceptor>();
    }

    /// <summary>
    /// Gets the types of interceptors that should be added to the DbContext.
    /// This method should be called when configuring the DbContext to add the appropriate interceptors.
    /// </summary>
    /// <returns>An enumerable of interceptor types to register with the DbContext.</returns>
    internal IEnumerable<Type> GetEnabledInterceptorTypes()
    {
        if (!_disabledFeatures.Contains("AuditableInterceptor"))
            yield return typeof(AuditableEntityInterceptor);

        if (!_disabledFeatures.Contains("DomainEventDispatch"))
            yield return typeof(DispatchDomainEventsInterceptor);
    }
}