namespace BuildingBlocks.Tests.Infrastructure.Containers;

public interface ITestContainer
{
    Task StartAsync();
    Task StopAsync();
    string GetConnectionString();
}
