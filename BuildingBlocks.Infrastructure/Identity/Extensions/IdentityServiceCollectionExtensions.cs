using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
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
        services.TryAddSingleton<RolePermissionService>();
        services.TryAddTransient<IClaimsTransformation, PermissionClaimsTransformation>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Clerk tokens do not include an 'aud' claim, so audience validation must be disabled.
                options.TokenValidationParameters.ValidateAudience = false;

                // In .NET 8+, MapInboundClaims defaults to false, meaning JWT claim names are preserved as-is (e.g. 'sub', 'roles'). 
                options.TokenValidationParameters.NameClaimType = ClaimNames.Sub;
                options.TokenValidationParameters.RoleClaimType = ClaimNames.Roles;
            });

        services.AddAuthorization();

        return services;
    }
}