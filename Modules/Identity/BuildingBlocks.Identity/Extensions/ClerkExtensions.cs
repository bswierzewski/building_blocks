using BuildingBlocks.Identity.Configuration;
using BuildingBlocks.Identity.Domain.Enums;
using BuildingBlocks.Identity.Infrastructure.Authentication;
using BuildingBlocks.Identity.Infrastructure.Authentication.Services;
using BuildingBlocks.Infrastructure.Modules.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace BuildingBlocks.Identity.Extensions;

/// <summary>
/// Registers Clerk JWT bearer authentication and prepares the normalized claims required by JIT provisioning.
/// </summary>
public static class ClerkExtensions
{
    /// <summary>
    /// Configures the default JWT bearer handler for Clerk-issued access tokens.
    /// </summary>
    public static IServiceCollection AddClerkJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<ClerkAuthenticationOptions>(configuration, ClerkAuthenticationOptions.SectionName);

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<ClerkAuthenticationOptions>>((options, clerkOptionsAccessor) =>
            {
                var clerkOptions = clerkOptionsAccessor.Value;

                options.Authority = clerkOptions.Authority;
                options.Audience = clerkOptions.Audience;
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "email",
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var azp = context.Principal?.FindFirst("azp")?.Value
                            ?? context.Principal?.FindFirst("client_id")?.Value;
                        // Clerk uses azp/client_id to identify the frontend application that requested the token.
                        if (!string.Equals(azp, clerkOptions.AuthorizedParty, StringComparison.Ordinal))
                        {
                            context.Fail("The token authorized party does not match the configured Clerk application.");
                            return;
                        }

                        if (context.Principal is null)
                        {
                            context.Fail("The authenticated principal is missing.");
                            return;
                        }

                        if (context.Principal.Identity is not ClaimsIdentity identity)
                        {
                            context.Fail("The authenticated identity is missing.");
                            return;
                        }

                        // Normalize provider-specific token fields into the shared internal claim contract.
                        PrepareJitClaims(identity, context.Principal, ExternalProvider.Clerk);

                        var jitProvisioningService = context.HttpContext.RequestServices.GetRequiredService<JitProvisioningService>();
                        await jitProvisioningService.ProvisionUserAsync(context.Principal, context.HttpContext.RequestAborted);
                    }
                };
            });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        return services;
    }

    /// <summary>
    /// Maps Clerk-specific claims to the normalized internal claim names consumed by JIT provisioning.
    /// </summary>
    private static void PrepareJitClaims(ClaimsIdentity identity, ClaimsPrincipal principal, ExternalProvider provider)
    {
        AddClaim(identity, IdentityClaimTypes.ExternalProvider, provider.ToString());

        // Clerk may expose the external subject either as sub or as a mapped name identifier.
        var externalUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
        if (!string.IsNullOrWhiteSpace(externalUserId))
            AddClaim(identity, IdentityClaimTypes.ExternalUserId, externalUserId);

        var email = principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;
        if (!string.IsNullOrWhiteSpace(email))
            AddClaim(identity, IdentityClaimTypes.Email, email);
    }

    /// <summary>
    /// Adds a normalized claim only when the principal does not already contain it.
    /// </summary>
    private static void AddClaim(ClaimsIdentity identity, string claimType, string value)
    {
        if (identity.HasClaim(claimType, value))
            return;

        identity.AddClaim(new Claim(claimType, value));
    }
}