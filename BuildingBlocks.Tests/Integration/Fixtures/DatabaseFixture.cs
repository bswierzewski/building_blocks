using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using Xunit;

namespace BuildingBlocks.Tests.Integration.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private Respawner? _respawner;
    private NpgsqlConnection? _dbConnection;

    public string ConnectionString => _dbContainer.GetConnectionString();

    public DatabaseFixture()
    {
        _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("integration_tests_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        _dbConnection = new NpgsqlConnection(ConnectionString);
        await _dbConnection.OpenAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        if (_dbConnection is null)
            throw new InvalidOperationException("The database fixture has not been initialized.");

        if (_respawner is null)
        {
            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public", "wolverine"],
                TablesToIgnore =
                [
                    new Table("public", "__EFMigrationsHistory")
                ]
            });
        }

        await _respawner.ResetAsync(_dbConnection);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbConnection is not null)
            await _dbConnection.DisposeAsync();

        await _dbContainer.DisposeAsync();
    }
}