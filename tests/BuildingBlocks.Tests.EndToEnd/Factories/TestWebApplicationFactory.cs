using BuildingBlocks.Tests.EndToEnd.Auth;
using BuildingBlocks.Tests.EndToEnd.Auth.Providers;
using BuildingBlocks.Tests.EndToEnd.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace BuildingBlocks.Tests.EndToEnd.Factories;

/// <summary>
/// Test web application factory that manages PostgreSQL test containers and database setup for E2E tests.
/// Provides isolated test databases with automatic cleanup and reset capabilities.
/// Generic version - subclasses can specify DbContext types specific to their domain.
/// </summary>
/// <typeparam name="TProgram">The Startup or Program type of the application being tested.</typeparam>
public abstract class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime, ITestWebApplicationFactory
    where TProgram : class
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithDatabase("postgres")
            .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;

    /// <summary>
    /// DbContext types that need to be migrated. Override in subclasses to specify specific contexts.
    /// </summary>
    protected virtual Type[] DbContextTypes => [];

    /// <summary>
    /// Initializes the test factory by starting the PostgreSQL container, applying migrations, and configuring the database respawner.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _connection = new NpgsqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();

        using (var scope = Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;

            // Migrate all registered DbContext types
            foreach (var contextType in DbContextTypes)
            {
                var dbContext = (DbContext?)serviceProvider.GetService(contextType);
                if (dbContext != null)
                {
                    await dbContext.Database.MigrateAsync();
                }
            }
        }

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"],
            WithReseed = true
        });
    }

    /// <summary>
    /// Disposes the test factory by closing the database connection and stopping the PostgreSQL container.
    /// </summary>
    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Resets all test databases to a clean state by truncating tables while preserving schema.
    /// </summary>
    public async Task ResetDatabasesAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    /// <summary>
    /// Configures the web host with test-specific settings including the test database connection and data protection.
    /// </summary>
    /// <param name="builder">The web host builder to configure.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var connectionString = _container.GetConnectionString();

            // Configure database contexts - override OnConfigureDbContexts to add specific contexts
            OnConfigureDbContexts(services, connectionString);

            // Use ephemeral (in-memory) Data Protection keys for tests.
            // Keys are generated automatically but not persisted, avoiding file system operations
            // and preventing key ring errors while maintaining encryption functionality.
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            // Allow subclasses to configure additional services
            OnConfigureServices(services);
        });
    }

    /// <summary>
    /// Override this method to configure specific DbContext instances with the test connection string.
    /// Example: services.ReplaceDbContext&lt;MyDbContext&gt;(connectionString);
    /// </summary>
    protected virtual void OnConfigureDbContexts(IServiceCollection services, string connectionString)
    {
        // Base implementation does nothing - subclasses override to configure their contexts
    }

    /// <summary>
    /// Configures common services for all test applications, including authentication provider selection.
    /// Override this method to configure additional services for the test application.
    /// IAuthTokenProvider is registered as singleton - provider is created once and caches token internally.
    /// Each provider validates its own configuration using DataAnnotations when instantiated.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    protected virtual void OnConfigureServices(IServiceCollection services)
    {
        // Configure provider selection - only this is loaded eagerly
        // Provider-specific options (Supabase, Clerk) are loaded lazily by providers themselves
        services.AddOptions<AuthProviderOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection(AuthProviderOptions.SectionName).Bind(options);
            });

        // Register IAuthTokenProvider as singleton
        // Provider is created once and validates its config in constructor (like ValidateOnStart)
        services.AddSingleton<IAuthTokenProvider>(sp =>
        {
            var providerOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthProviderOptions>>();
            var configuration = sp.GetRequiredService<IConfiguration>();

            return providerOptions.Value.Provider switch
            {
                AuthProvider.None => new NoneAuthTokenProvider(),
                AuthProvider.Supabase => new SupabaseAuthTokenProvider(configuration),
                AuthProvider.Clerk => new ClerkAuthTokenProvider(configuration),
                _ => throw new InvalidOperationException($"Unknown provider: {providerOptions.Value.Provider}")
            };
        });
    }

    /// <summary>
    /// Implements ITestWebApplicationFactory.WithWebHostBuilder
    /// </summary>
    ITestWebApplicationFactory ITestWebApplicationFactory.WithWebHostBuilder(Action<IWebHostBuilder> configure)
    {
        return (ITestWebApplicationFactory)WithWebHostBuilder(configure);
    }
}
