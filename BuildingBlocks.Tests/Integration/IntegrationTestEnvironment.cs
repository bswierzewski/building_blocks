using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using Xunit;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Shared runtime environment for one integration-test stack.
/// Manages the PostgreSQL container, schema initialization, and Respawn reset state.
/// </summary>
public abstract class IntegrationTestEnvironment<TProgram> : IAsyncLifetime
    where TProgram : class
{
  private readonly SemaphoreSlim _resetLock = new(1, 1);

  private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
      .WithImage("postgres:18-alpine")
      .WithDatabase("db")
      .WithUsername("postgres")
      .WithPassword("postgres")
      .Build();

  private NpgsqlConnection _connection = default!;
  private Respawner _respawner = default!;

  public string ConnectionString => _postgresContainer.GetConnectionString();

  /// <summary>
  /// Starts the shared PostgreSQL container, initializes the schema, and prepares Respawn.
  /// </summary>
  public async ValueTask InitializeAsync()
  {
    await _postgresContainer.StartAsync();

    _connection = new NpgsqlConnection(ConnectionString);
    await _connection.OpenAsync();

    await InitializeDatabaseAsync();

    _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
    {
      DbAdapter = DbAdapter.Postgres,
      TablesToIgnore = [new Table("__EFMigrationsHistory")],
      WithReseed = true
    });
  }

  /// <summary>
  /// Resets the shared database state so each test starts from a clean baseline.
  /// </summary>
  public async Task ResetDatabaseAsync()
  {
    await _resetLock.WaitAsync();

    try
    {
      await _respawner.ResetAsync(_connection);
    }
    finally
    {
      _resetLock.Release();
    }
  }

  /// <summary>
  /// Disposes the shared PostgreSQL resources for the current test collection.
  /// </summary>
  public async ValueTask DisposeAsync()
  {
    await _connection.DisposeAsync();
    await _postgresContainer.DisposeAsync();
    _resetLock.Dispose();
  }

  /// <summary>
  /// Override to register collection-wide service replacements applied to every test in this environment.
  /// </summary>
  public virtual void ConfigureServices(IServiceCollection services) { }

  /// <summary>
  /// Override to initialize the database schema before Respawn inspects tables.
  /// This is the right place to run application startup that applies EF Core migrations.
  /// </summary>
  protected virtual ValueTask InitializeDatabaseAsync()
    => ValueTask.CompletedTask;
}