using BuildingBlocks.Tests.Core;
using BuildingBlocks.Tests.Infrastructure.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Tests.Builders;

internal class TestHostBuilder<TProgram> where TProgram : class
{
    private readonly List<Action<IServiceCollection, IConfiguration>> _serviceConfigurations = new();
    private readonly List<Action<IWebHostBuilder>> _hostConfigurations = new();
    private ITestContainer? _container;
    private string _environment = "Testing";


    public TestHostBuilder<TProgram> WithContainer(ITestContainer? container)
    {
        _container = container;
        return this;
    }

    public TestHostBuilder<TProgram> WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    public TestHostBuilder<TProgram> WithServices(Action<IServiceCollection, IConfiguration> configure)
    {
        _serviceConfigurations.Add(configure);
        return this;
    }

    public TestHostBuilder<TProgram> WithHostConfiguration(Action<IWebHostBuilder> configure)
    {
        _hostConfigurations.Add(configure);
        return this;
    }

    public TestHost<TProgram> Build()
    {
        return new TestHost<TProgram>(
            _container,
            _serviceConfigurations,
            _hostConfigurations,
            _environment);
    }
}
