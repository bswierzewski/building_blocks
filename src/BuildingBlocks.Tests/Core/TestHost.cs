using BuildingBlocks.Tests.Extensions.Npgsql;
using BuildingBlocks.Tests.Infrastructure.Containers;
using BuildingBlocks.Tests.Infrastructure.Database;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Tests.Core;

internal class TestHost<TProgram> : WebApplicationFactory<TProgram>, ITestHost where TProgram : class
{
    private readonly ITestContainer? _container;
    private readonly List<Action<IServiceCollection, IConfiguration>> _serviceConfigurations = new();
    private readonly List<Action<IWebHostBuilder>> _hostConfigurations = new();
    private DatabaseResetStrategy? _resetStrategy;
    private string _environment = "Testing";

    public TestHost(
        ITestContainer? container,
        IEnumerable<Action<IServiceCollection, IConfiguration>> serviceConfigurations,
        IEnumerable<Action<IWebHostBuilder>> hostConfigurations,
        string environment)
    {
        _container = container;
        _serviceConfigurations.AddRange(serviceConfigurations);
        _hostConfigurations.AddRange(hostConfigurations);
        _environment = environment;
    }

    internal void SetResetStrategy(DatabaseResetStrategy strategy)
    {
        _resetStrategy = strategy;
    }

    public async Task ResetDatabaseAsync()
    {
        if (_resetStrategy != null)
            await _resetStrategy.ResetAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);

        builder.ConfigureLogging((context, logging) =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        foreach (var configure in _hostConfigurations)
            configure(builder);

        builder.ConfigureServices((context, services) =>
        {
            if (_container != null)
                services.ReplaceNpgsqlDataSources(_container.GetConnectionString());

            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            services.AddSingleton<IDatabaseManager, DatabaseManager>();

            foreach (var configure in _serviceConfigurations)
                configure(services, context.Configuration);
        });
    }

    public new async ValueTask DisposeAsync()
    {
        if (_resetStrategy != null)
            await _resetStrategy.DisposeAsync();

        await base.DisposeAsync();
    }
}
