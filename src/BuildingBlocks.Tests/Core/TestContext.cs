using BuildingBlocks.Tests.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Tests.Core;

public sealed class TestContext : IAsyncDisposable
{
    private readonly ITestHost _host;
    private HttpClient? _client;

    internal TestContext(ITestHost host)
    {
        _host = host;
    }

    public HttpClient Client => _client ??= _host.CreateClient();

    public IServiceProvider Services => _host.Services;

    public Task ResetDatabaseAsync() => _host.ResetDatabaseAsync();

    public IServiceScope CreateScope() => Services.CreateScope();

    public T GetRequiredService<T>() where T : notnull
        => Services.GetRequiredService<T>();
    public T? GetService<T>() where T : class
        => Services.GetService<T>();
    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        await _host.DisposeAsync();
    }
    public static TestContextBuilder<TProgram> CreateBuilder<TProgram>()
        where TProgram : class
        => new TestContextBuilder<TProgram>();
}
