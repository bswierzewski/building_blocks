using BuildingBlocks.Abstractions.Abstractions;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class UserContextExtensions
{
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}
