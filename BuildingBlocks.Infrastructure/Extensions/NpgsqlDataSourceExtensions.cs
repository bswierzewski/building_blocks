using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class NpgsqlDataSourceExtensions
{
    public static NpgsqlDataSource AddNpgsqlDataSource(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "Default")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");

        var dataSource = new NpgsqlDataSourceBuilder(connectionString)
            .EnableDynamicJson()
            .Build();

        services.AddSingleton(dataSource);

        return dataSource;
    }
}
