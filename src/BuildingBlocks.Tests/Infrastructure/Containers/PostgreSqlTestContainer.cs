using Testcontainers.PostgreSql;

namespace BuildingBlocks.Tests.Infrastructure.Containers;

public class PostgreSqlTestContainer : ITestContainer
{
    private readonly PostgreSqlContainer _container;

    public PostgreSqlTestContainer()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithDatabase("postgres")
            .Build();
    }

    public Task StartAsync() => _container.StartAsync();
    public Task StopAsync() => _container.DisposeAsync().AsTask();
    public string GetConnectionString() => _container.GetConnectionString();
}
