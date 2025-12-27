namespace BuildingBlocks.Tests.Core;

public interface ITestHost : IAsyncDisposable
{
    IServiceProvider Services { get; }
    HttpClient CreateClient();
    Task ResetDatabaseAsync();
}
