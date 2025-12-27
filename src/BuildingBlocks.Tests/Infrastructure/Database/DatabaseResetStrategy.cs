using Npgsql;
using Respawn;

namespace BuildingBlocks.Tests.Infrastructure.Database;

public class DatabaseResetStrategy
{
    private readonly List<string> _tablesToIgnore = new() { "__EFMigrationsHistory" };
    private Respawner? _respawner;
    private NpgsqlConnection? _connection;

    public void IgnoreTables(params string[] tables)
        => _tablesToIgnore.AddRange(tables);

    public async Task InitializeAsync(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = _tablesToIgnore
                .Select(t => new Respawn.Graph.Table(t))
                .ToArray(),
            WithReseed = true
        });
    }

    public async Task ResetAsync()
    {
        if (_respawner != null && _connection != null)
        {
            await _respawner.ResetAsync(_connection);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}
