using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using Xunit;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Base class for a single xUnit collection. Manages the PostgreSQL container and database reset state.
/// Derive and add ICollectionFixture&lt;DerivedClass&gt; + [CollectionDefinition] on the derived class.
/// Override ConfigureServices to register collection-wide service replacements.
/// </summary>
public abstract class IntegrationTestCollection<TProgram> : IAsyncLifetime
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

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _postgresContainer.DisposeAsync();
        _resetLock.Dispose();
    }

    /// <summary>
    /// Override to register collection-wide service replacements applied to every test in this collection.
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services) { }

    /// <summary>
    /// Override to initialize the database schema before Respawn inspects tables.
    /// This is the right place to run application startup that applies EF Core migrations.
    /// </summary>
    protected virtual ValueTask InitializeDatabaseAsync()
      => ValueTask.CompletedTask;
}
