using Alba;
using Alba.Security;
using BuildingBlocks.Tests.Integration.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BuildingBlocks.Tests.Integration;

public abstract class IntegrationTestBase<TEntryPoint>(DatabaseFixture databaseFixture) : IAsyncLifetime where TEntryPoint : class
{
    private readonly DatabaseFixture _dbFixture = databaseFixture;
    private readonly JwtSecurityStub _jwtSecurity = new();

    public IAlbaHost Host { get; private set; } = default!;

    protected virtual void OnConfigureServices(IServiceCollection services) { }
    protected virtual Task OnInitializeAsync(IServiceProvider services) => Task.CompletedTask;
    protected virtual Task OnDisposeAsync() => Task.CompletedTask;

    public Task ResetDatabaseAsync() => _dbFixture.ResetDatabaseAsync();

    public async ValueTask InitializeAsync()
    {
        Host = await AlbaHost.For<TEntryPoint>(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = _dbFixture.ConnectionString
                });
            });

            builder.ConfigureServices((_, services) => OnConfigureServices(services));

        }, _jwtSecurity);

        await ResetDatabaseAsync();
        await OnInitializeAsync(Host.Services);
    }

    public async ValueTask DisposeAsync()
    {
        await OnDisposeAsync();

        if (Host != null)
            await Host.DisposeAsync();
    }
}