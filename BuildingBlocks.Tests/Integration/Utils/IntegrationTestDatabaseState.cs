using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace BuildingBlocks.Tests.Integration.Utils;

/// <summary>
/// Owns the PostgreSQL test container and Respawn state used by one integration-test environment.
/// </summary>
public sealed class IntegrationTestDatabaseState : IAsyncDisposable
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
  /// Starts the database container, runs the supplied initialization logic, and snapshots the baseline state.
  /// </summary>
  public async Task InitializeAsync(Func<string, Task>? initializeDatabaseAsync = null)
  {
    await _postgresContainer.StartAsync();

    _connection = new NpgsqlConnection(ConnectionString);
    await _connection.OpenAsync();

    if (initializeDatabaseAsync is not null)
      await initializeDatabaseAsync(ConnectionString);

    _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
    {
      DbAdapter = DbAdapter.Postgres,
      TablesToIgnore = [new Table("__EFMigrationsHistory")],
      WithReseed = true
    });
  }

  /// <summary>
  /// Resets the database to the baseline snapshot for the next test.
  /// </summary>
  public async Task ResetAsync()
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

  public async ValueTask DisposeAsync()
  {
    if (_connection is not null)
      await _connection.DisposeAsync();

    await _postgresContainer.DisposeAsync();
    _resetLock.Dispose();
  }
}