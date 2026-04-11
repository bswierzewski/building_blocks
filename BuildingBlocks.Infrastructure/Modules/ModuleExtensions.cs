using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using BuildingBlocks.Infrastructure.Persistence.Interceptors;

namespace BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// Provides shared service registration helpers for application modules.
/// </summary>
public static class ModuleExtensions
{
  /// <summary>
  /// Registers a PostgreSQL-backed DbContext with audit interceptors and a schema-specific migrations history table.
  /// </summary>
  public static IServiceCollection AddPostgres<TDbContext>(this IServiceCollection services, string schema)
      where TDbContext : DbContext
  {
    services.TryAddScoped<AuditableEntityInterceptor>();

    services.AddDbContext<TDbContext>((sp, options) =>
    {
      var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
      options.AddInterceptors(auditableInterceptor);

      var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
      options.UseNpgsql(dataSource, npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema));
    });

    return services;
  }

  /// <summary>
  /// Binds options from configuration, enables data annotation validation, and validates them on application startup.
  /// </summary>
  public static IServiceCollection AddValidatedOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string path)
      where TOptions : class
  {
    services.AddOptions<TOptions>()
        .Bind(configuration.GetSection(path))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    return services;
  }

  /// <summary>
  /// Registers a custom options validator and applies the standard validated options binding pipeline.
  /// </summary>
  public static IServiceCollection AddValidatedOptions<TOptions, TValidator>(this IServiceCollection services, IConfiguration configuration, string path)
      where TOptions : class
      where TValidator : class, IValidateOptions<TOptions>
  {
    services.AddSingleton<IValidateOptions<TOptions>, TValidator>();

    return services.AddValidatedOptions<TOptions>(configuration, path);
  }
}
