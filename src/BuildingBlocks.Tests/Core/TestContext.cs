using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Kernel.Abstractions;
using BuildingBlocks.Tests.Extensions.Npgsql;
using BuildingBlocks.Tests.Infrastructure.Authentication;
using BuildingBlocks.Tests.Infrastructure.Containers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;

namespace BuildingBlocks.Tests.Core;

public sealed class TestContext<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    private readonly ITestContainer _container;
    private readonly List<Action<IServiceCollection, IConfiguration>> _serviceConfigurations = new();
    private Respawner? _respawner;
    private NpgsqlConnection? _connection;
    private bool _initializeModules = true;
    private readonly List<string> _tablesToIgnore = new() { "__EFMigrationsHistory" };
    private string _environment = "Testing";
    private HttpClient? _client;

    public TestContext(ITestContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public HttpClient Client => _client ??= CreateClient();

    public TestContext<TProgram> WithTestAuthentication()
    {
        return WithServices((services, _) =>
        {
            services.AddAuthentication(TestAuthenticationHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.AuthenticationScheme, null);
            services.AddScoped<IUserContext, TestUserContext>();
        });
    }

    public TestContext<TProgram> WithServices(Action<IServiceCollection, IConfiguration> configure)
    {
        _serviceConfigurations.Add(configure);
        return this;
    }

    public TestContext<TProgram> WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    public TestContext<TProgram> WithoutModuleInitialization()
    {
        _initializeModules = false;
        return this;
    }

    public TestContext<TProgram> WithTablesIgnoredOnReset(params string[] tables)
    {
        _tablesToIgnore.AddRange(tables);
        return this;
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

        builder.ConfigureServices((context, services) =>
        {
            // Replace database connections with test container
            services.ReplaceNpgsqlDataSources(_container.GetConnectionString());

            // Use ephemeral data protection for tests
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            // Apply custom service configurations
            foreach (var configure in _serviceConfigurations)
            {
                configure(services, context.Configuration);
            }
        });
    }

    public async Task InitializeAsync()
    {
        // Initialize modules (run migrations)
        if (_initializeModules)
        {
            await Services.InitModules();
        }

        // Initialize Respawner for database cleanup
        _connection = new NpgsqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = _tablesToIgnore
                .Select(t => new Respawn.Graph.Table(t))
                .ToArray(),
            WithReseed = true
        });

        // Reset database to clean state
        await ResetDatabaseAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner != null && _connection != null)
        {
            await _respawner.ResetAsync(_connection);
        }
    }

    public IServiceScope CreateScope() => Services.CreateScope();

    public T GetRequiredService<T>() where T : notnull
        => Services.GetRequiredService<T>();

    public T? GetService<T>() where T : class
        => Services.GetService<T>();

    async Task IAsyncLifetime.DisposeAsync()
    {
        _client?.Dispose();

        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
