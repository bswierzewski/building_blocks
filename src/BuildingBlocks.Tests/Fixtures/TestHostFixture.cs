using BuildingBlocks.Tests.Builders;
using BuildingBlocks.Tests.Core;
using BuildingBlocks.Tests.Infrastructure.Containers;

namespace BuildingBlocks.Tests.Fixtures;

public class TestHostFixture<TProgram>(
    Func<TestContextBuilder<TProgram>, TestContextBuilder<TProgram>>? configure = null) 
    : IAsyncLifetime, ITestHostFixture where TProgram : class
{
    private readonly Func<TestContextBuilder<TProgram>, TestContextBuilder<TProgram>> _configure = configure ?? (builder => builder);

    public PostgreSqlTestContainer Container { get; } = new();
    public TestContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        var builder = TestContext.CreateBuilder<TProgram>()
            .WithContainer(Container);
        builder = _configure(builder);
        Context = await builder.BuildAsync();
    }

    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.DisposeAsync();
        }

        await Container.StopAsync();
    }
}
