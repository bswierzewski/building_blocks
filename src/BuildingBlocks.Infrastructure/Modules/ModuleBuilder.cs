using BuildingBlocks.Infrastructure.Behaviors;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Reflection;

namespace BuildingBlocks.Infrastructure.Modules;

public class ModuleBuilder(IServiceCollection services, IConfiguration configuration, string moduleName)
{
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    public IConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));
    public string ModuleName { get; } = moduleName ?? throw new ArgumentNullException(nameof(moduleName));

    public ModuleBuilder AddOptions(Action<IServiceCollection, IConfiguration> configureOptions)
    {
        configureOptions(Services, Configuration);

        return this;
    }

    public ModuleBuilder AddPostgres<TDbContext, TInterface>(
        Func<IServiceProvider, string> connectionStringFactory)
        where TDbContext : DbContext, TInterface
        where TInterface : class
    {
        Services.AddKeyedSingleton(ModuleName, (sp, key) =>
        {
            var connectionString = connectionStringFactory(sp);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException($"Connection string for module '{ModuleName}' is empty.");

            return NpgsqlDataSource.Create(connectionString);
        });
        
        Services.AddScoped<SaveChangesInterceptor, AuditableEntityInterceptor>();
        Services.AddScoped<SaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        Services.AddDbContext<TDbContext>((sp, options) =>
        {
            var dataSource = sp.GetRequiredKeyedService<NpgsqlDataSource>(ModuleName);

            options.UseNpgsql(dataSource)
                   .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });

        Services.AddScoped<TInterface>(sp => sp.GetRequiredService<TDbContext>());

        return this;
    }

    public ModuleBuilder AddCQRS(params Assembly[] assemblies)
    {
        Services.AddMediatR(config =>
        {
            foreach (var assembly in assemblies)
                config.RegisterServicesFromAssembly(assembly);
                
            config.AddOpenRequestPreProcessor(typeof(LoggingBehavior<>));
            config.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        foreach (var assembly in assemblies)
            Services.AddValidatorsFromAssembly(assembly);

        return this;
    }

    public IServiceCollection Build() => Services;
}
