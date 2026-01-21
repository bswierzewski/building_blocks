using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace BuildingBlocks.Tests.Extensions.Services;

public static class ServiceCollectionExtensions
{
    public static Mock<TService> ReplaceMock<TService>(
        this IServiceCollection services,
        MockBehavior behavior = MockBehavior.Default)
        where TService : class
    {
        var mock = new Mock<TService>(behavior);
        services.RemoveAll<TService>();
        services.AddSingleton(mock.Object);
        return mock;
    }

    public static IServiceCollection ReplaceService<TService, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService : class
        where TImplementation : class, TService
    {
        services.RemoveAll<TService>();

        return lifetime switch
        {
            ServiceLifetime.Singleton => services.AddSingleton<TService, TImplementation>(),
            ServiceLifetime.Scoped => services.AddScoped<TService, TImplementation>(),
            ServiceLifetime.Transient => services.AddTransient<TService, TImplementation>(),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime))
        };
    }

    public static IServiceCollection ReplaceInstance<TService>(
        this IServiceCollection services,
        TService instance)
        where TService : class
    {
        services.RemoveAll<TService>();
        services.AddSingleton(instance);
        return services;
    }
}
