using Npgsql;
using Testcontainers.PostgreSql;

namespace BuildingBlocks.Tests.Integration.Containers;

/// <summary>
/// Thin wrapper around a PostgreSQL Testcontainer used by integration tests.
/// </summary>
public sealed class PostgresTestContainer : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;

    public PostgresTestContainer()
    {
        _container = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("scaffold")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public Task StartAsync(CancellationToken cancellationToken = default)
        => _container.StartAsync(cancellationToken);

    public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public ValueTask DisposeAsync()
        => _container.DisposeAsync();
}