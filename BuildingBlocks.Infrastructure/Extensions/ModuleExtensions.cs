using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using BuildingBlocks.Kernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Wolverine.EntityFrameworkCore;

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
        var schema = ModuleName.ToLowerInvariant();

        Services.TryAddScoped<AuditableEntityInterceptor>();

        Services.AddDbContextWithWolverineIntegration<TDbContext>((sp, options) =>
        {
            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema);
            });
            options.AddInterceptors(auditableInterceptor);
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