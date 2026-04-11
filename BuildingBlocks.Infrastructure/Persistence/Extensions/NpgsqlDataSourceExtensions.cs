using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Persistence.Extensions;

/// <summary>
/// Provides helpers for registering a shared Npgsql data source from configuration.
/// </summary>
public static class NpgsqlDataSourceExtensions
{
  /// <summary>
  /// Builds and registers a singleton Npgsql data source using the specified connection string.
  /// </summary>
  public static NpgsqlDataSource AddNpgsqlDataSource(
      this IServiceCollection services,
      IConfiguration configuration,
      string connectionStringName = "Default")
  {
    var connectionString = configuration.GetConnectionString(connectionStringName);

    if (string.IsNullOrWhiteSpace(connectionString))
      throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");

    var dataSource = new NpgsqlDataSourceBuilder(connectionString)
        .EnableDynamicJson()
        .Build();

    services.AddSingleton(dataSource);

    return dataSource;
  }
}
