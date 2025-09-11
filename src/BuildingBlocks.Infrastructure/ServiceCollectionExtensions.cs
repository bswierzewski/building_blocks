using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Infrastructure.Persistence.Migrations;

namespace BuildingBlocks.Infrastructure;

/// <summary>
/// Extension methods for IServiceCollection to add infrastructure layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds migration service for the specified DbContext.
    /// The service runs on application startup to check for and apply pending migrations.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMigrationService<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        return services.AddHostedService<MigrationService<TContext>>();
    }

    /// <summary>
    /// Adds the auditable entity interceptor.
    /// Automatically populates CreatedAt, CreatedBy, ModifiedAt, ModifiedBy fields for entities implementing IAuditable.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAuditableEntityInterceptor(this IServiceCollection services)
    {
        return services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
    }

    /// <summary>
    /// Adds the domain event dispatch interceptor.
    /// Automatically publishes domain events from aggregate roots via MediatR after SaveChanges.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddDomainEventDispatchInterceptor(this IServiceCollection services)
    {
        return services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
    }
}