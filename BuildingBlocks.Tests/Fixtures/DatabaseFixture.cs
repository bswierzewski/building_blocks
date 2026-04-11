using Testcontainers.PostgreSql;
using Xunit;

namespace BuildingBlocks.IntegrationTests.Fixtures;

/// <summary>
/// xUnit fixture that manages a PostgreSQL container for integration tests.
/// Use with ICollectionFixture to share a single container across all test classes.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _postgresContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
