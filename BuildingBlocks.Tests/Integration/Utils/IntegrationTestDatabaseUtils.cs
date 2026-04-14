using BuildingBlocks.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Tests.Integration.Utils;

/// <summary>
/// Provides helper methods for preparing integration-test database schema before Respawn snapshots are created.
/// </summary>
public static class IntegrationTestDatabaseUtils
{
  private const string MigrationsHistoryTableName = "__EFMigrationsHistory";

  /// <summary>
  /// Applies pending EF Core migrations for the specified DbContext using a schema-specific
  /// migrations history table.
  /// </summary>
  public static async Task MigrateDatabaseAsync<TContext>(string connectionString)
      where TContext : DbContext
  {
    var normalizedSchema = typeof(TContext).ToDbContextSchemaName();

    var options = new DbContextOptionsBuilder<TContext>()
        .UseNpgsql(connectionString, np =>
            np.MigrationsHistoryTable(MigrationsHistoryTableName, normalizedSchema))
        .Options;

    if (Activator.CreateInstance(typeof(TContext), options) is not TContext dbContext)
      throw new InvalidOperationException($"Could not create {typeof(TContext).Name} for integration-test migration setup.");

    await using (dbContext)
      await dbContext.Database.MigrateAsync();
  }
}