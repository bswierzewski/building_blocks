using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Kernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class ModuleExtensions
{
    public static ModuleBuilder AddModule(this IServiceCollection services, IConfiguration configuration, string moduleName)
        => new(services, configuration, moduleName);
}

public sealed class ModuleBuilder(IServiceCollection services, IConfiguration configuration, string moduleName)
{
    public IServiceCollection Services { get; } = services;
    public IConfiguration Configuration { get; } = configuration;
    public string ModuleName { get; } = moduleName;

    public ModuleBuilder AddPostgres<TDbContext>()
        where TDbContext : DbContext
    {
        Services.AddKeyedSingleton(ModuleName, (sp, key) =>
        {
            var connectionString = Configuration.GetConnectionString(ModuleName);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string '{ModuleName}' not found in configuration.");

            return new NpgsqlDataSourceBuilder(connectionString)
                .EnableDynamicJson()
                .Build();
        });

        Services.TryAddScoped<AuditableEntityInterceptor>();

        Services.AddDbContext<TDbContext>((sp, options) =>
        {
            var dataSource = sp.GetRequiredKeyedService<NpgsqlDataSource>(ModuleName);
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.AddInterceptors(auditableInterceptor);
        });

        return this;
    }

    public ModuleBuilder AddSqlite<TDbContext>()
        where TDbContext : DbContext
    {
        Services.TryAddScoped<AuditableEntityInterceptor>();

        Services.AddDbContext<TDbContext>((sp, options) =>
        {
            var connectionString = Configuration.GetConnectionString(ModuleName);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string '{ModuleName}' not found in configuration.");

            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.AddInterceptors(auditableInterceptor);
            options.UseSqlite(connectionString);
        });

        return this;
    }

    public ModuleBuilder AddOptions(Action<ModuleBuilder> configure)
    {
        configure(this);

        return this;
    }

    public ModuleBuilder ConfigureOptions<T>() where T : class, IOptions
    {
        Services.Configure<T>(Configuration.GetSection(T.SectionName));

        return this;
    }

    public IServiceCollection Build() => Services;
}