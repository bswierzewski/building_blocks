using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BuildingBlocks.Tests.Extensions.Npgsql;

public static class NpgsqlDataSourceExtensions
{
    public static void ReplaceNpgsqlDataSources(this IServiceCollection services, string testConnectionString)
    {
        var descriptors = services
            .Where(s => s.ServiceType == typeof(NpgsqlDataSource))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);

            if (descriptor.IsKeyedService)
            {
                services.AddKeyedSingleton(
                    descriptor.ServiceKey,
                    (_, _) => CreateDataSource(testConnectionString));
            }
            else
            {
                services.AddSingleton(
                    _ => CreateDataSource(testConnectionString));
            }
        }
    }

    private static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        return builder.Build();
    }
}
