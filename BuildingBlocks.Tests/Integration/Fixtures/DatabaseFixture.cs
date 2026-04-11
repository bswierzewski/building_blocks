using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace BuildingBlocks.Tests.Integration.Fixtures;

/// <summary>
/// xUnit fixture that manages a PostgreSQL container and database reset state for integration tests.
/// Use this through a single test collection to share one database stack across all integration test classes.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
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

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = [new Respawn.Graph.Table("__EFMigrationsHistory")],
            WithReseed = true
        });
    }

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

    public async Task DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _postgresContainer.DisposeAsync();
        _resetLock.Dispose();
    }
}
