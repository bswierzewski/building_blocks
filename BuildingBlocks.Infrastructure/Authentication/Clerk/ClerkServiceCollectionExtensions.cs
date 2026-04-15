using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Infrastructure.Authentication.Users;
using BuildingBlocks.Infrastructure.Modules;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Infrastructure.Authentication.Clerk;

/// <summary>
/// Registers Clerk authentication services used by the HTTP API.
/// </summary>
public static class ClerkServiceCollectionExtensions
{
    public static IServiceCollection AddClerkAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionPath = ClerkAuthenticationOptions.SectionPath)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUser, HttpContextCurrentUser>();

        services.AddValidatedOptions<ClerkAuthenticationOptions>(configuration, sectionPath);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddAuthorization();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<ClerkAuthenticationOptions>, IServiceProvider>((options, clerkOptionsAccessor, _) =>
            {
                var clerkOptions = clerkOptionsAccessor.Value;

                options.MapInboundClaims = false;
                options.Authority = string.IsNullOrWhiteSpace(clerkOptions.Authority)
                    ? null
                    : clerkOptions.Authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(clerkOptions.Authority),
                    ValidIssuer = clerkOptions.Authority,
                    ValidateAudience = true,
                    ValidAudience = clerkOptions.Audience,
                    ValidateLifetime = true,
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });

        return services;
    }
}