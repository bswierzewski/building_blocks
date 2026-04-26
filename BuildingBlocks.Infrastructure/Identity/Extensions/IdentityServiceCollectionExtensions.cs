using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Infrastructure.Identity;

/// <summary>
/// Registers authentication, authorization, and current-user services.
/// </summary>
public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// Adds JWT-based identity services backed by module-published roles.
    /// </summary>
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.TryAddScoped<ICurrentUser, CurrentUser>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddAuthorization();

        return services;
    }
}