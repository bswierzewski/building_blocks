using BuildingBlocks.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BuildingBlocks.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Provides a convention-based design-time factory for module DbContexts.
/// </summary>
public abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
  private const string DesignTimeConnectionString = "Host=_design-time_;Database=_design-time_";

  /// <summary>
  /// Creates a new DbContext instance configured for EF Core design-time tooling.
  /// </summary>
  public TContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<TContext>();

    optionsBuilder.UseNpgsql(
        connectionString: DesignTimeConnectionString,
        npgsqlOptionsAction: o => o.MigrationsHistoryTable("__EFMigrationsHistory", typeof(TContext).ToDbContextSchemaName())
    );

    if (Activator.CreateInstance(typeof(TContext), optionsBuilder.Options) is not TContext dbContext)
      throw new InvalidOperationException($"Could not create {typeof(TContext).Name} using the expected DbContextOptions constructor.");

    return dbContext;
  }
}