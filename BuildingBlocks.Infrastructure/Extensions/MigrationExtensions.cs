using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class MigrationExtensions
{
    /// <summary>
    /// Checks for and applies any pending Entity Framework Core database migrations for the specified DbContext.
    /// Used during application startup to automatically update the database schema.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext to migrate</typeparam>
    /// <param name="host">The application host</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <exception cref="Exception">Re-throws any database migration errors after logging</exception>
    public static async Task MigrateDatabaseAsync<TContext>(this IHost host, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
        var logger = host.Services.GetRequiredService<ILogger<TContext>>();

        // Create a new scope to ensure DbContext instance is properly disposed
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            var contextName = typeof(TContext).Name;
            logger.LogInformation("Checking migrations for {ContextName}...", contextName);

            // Check for any migrations that haven't been applied to the database
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Found {Count} pending migrations for {ContextName}: [{Migrations}]",
                    pendingMigrations.Count(), contextName, string.Join(", ", pendingMigrations));

                // Apply all pending migrations
                await context.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("{ContextName} database migrations applied successfully", contextName);
            }
            else
            {
                logger.LogInformation("No pending migrations for {ContextName} - database is up to date", contextName);
            }
        }
        catch (Exception ex)
        {
            var contextName = typeof(TContext).Name;
            logger.LogError(ex, "Error occurred during {ContextName} database migration", contextName);
            throw;
        }
    }
}