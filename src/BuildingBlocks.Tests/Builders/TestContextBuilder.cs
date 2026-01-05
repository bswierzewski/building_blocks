using BuildingBlocks.Abstractions.Abstractions;
using BuildingBlocks.Infrastructure.Modules;
using BuildingBlocks.Tests.Core;
using BuildingBlocks.Tests.Infrastructure.Authentication;
using BuildingBlocks.Tests.Infrastructure.Containers;
using BuildingBlocks.Tests.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Tests.Builders;

public sealed class TestContextBuilder<TProgram> where TProgram : class
{
    private readonly List<Action<IServiceCollection, IConfiguration>> _serviceConfigurations = new();
    private readonly List<Action<IWebHostBuilder>> _hostConfigurations = new();
    private readonly DatabaseResetStrategy _resetStrategy = new();
    private string _environment = "Testing";
    private ITestContainer? _container;
    private bool _autoInitializeModules = true;

    internal TestContextBuilder() { }

    public TestContextBuilder<TProgram> WithContainer(ITestContainer container)
    {
        ArgumentNullException.ThrowIfNull(container, nameof(container));
        _container = container;
        return this;
    }

    public TestContextBuilder<TProgram> WithTablesIgnoredOnReset(params string[] tables)
    {
        _resetStrategy.IgnoreTables(tables);
        return this;
    }

    public TestContextBuilder<TProgram> WithServices(
        Action<IServiceCollection, IConfiguration> configure)
    {
        _serviceConfigurations.Add(configure);
        return this;
    }

    public TestContextBuilder<TProgram> WithHostConfiguration(
        Action<IWebHostBuilder> configure)
    {
        _hostConfigurations.Add(configure);
        return this;
    }

    public TestContextBuilder<TProgram> WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    public TestContextBuilder<TProgram> WithoutModuleInitialization()
    {
        _autoInitializeModules = false;
        return this;
    }

    public TestContextBuilder<TProgram> WithTestAuthentication()
    {
        return WithServices((services, _) =>
        {
            // Register test authentication handler for bypassing authorization in tests
            services.AddAuthentication(TestAuthenticationHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.AuthenticationScheme, null);

            // Replace IUserContext with test implementation
            services.AddScoped<IUserContext, TestUserContext>();
        });
    }

    public async Task<TestContext> BuildAsync()
    {
        if (_container == null)
        {
            throw new InvalidOperationException(
                "No container configured. Call WithContainer() before BuildAsync(). " +
                "Example: .WithContainer(fixture.Container)");
        }

        // Get connection string from container
        var connectionString = _container.GetConnectionString();

        // Build host
        var hostBuilder = new TestHostBuilder<TProgram>()
            .WithEnvironment(_environment)
            .WithContainer(_container);

        // Apply all host configurations
        foreach (var configure in _hostConfigurations)
        {
            hostBuilder.WithHostConfiguration(configure);
        }

        // Apply all service configurations
        foreach (var configure in _serviceConfigurations)
        {
            hostBuilder.WithServices(configure);
        }

        var host = hostBuilder.Build();

        // Initialize modules (migrations, permissions sync, etc.)
        if (_autoInitializeModules)
        {
            var modules = host.Services.GetServices<IModule>();
            foreach (var module in modules)
            {
                await module.Initialize(host.Services, CancellationToken.None);
            }
        }

        // Initialize database reset strategy (Respawn)
        await _resetStrategy.InitializeAsync(connectionString);

        // Set the reset strategy on the host for later use
        host.SetResetStrategy(_resetStrategy);

        // Reset database to clean state
        await host.ResetDatabaseAsync();

        return new TestContext(host);
    }
}
