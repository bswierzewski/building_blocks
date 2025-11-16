using System.Security.Claims;
using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Application.Options;
using BuildingBlocks.Modules.Users.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Modules.Users.Web.Extensions.JwtBearers;

/// <summary>
/// Extension methods for adding Clerk JWT Bearer authentication.
/// </summary>
public static class ClerkJwtBearerExtensions
{
    /// <summary>
    /// Adds Clerk JWT Bearer authentication with user provisioning.
    /// Configures JWT validation and enriches claims with user ID, roles, and permissions from database.
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="configureOptions">Optional additional JWT Bearer configuration</param>
    /// <returns>The authentication builder for chaining</returns>
    public static AuthenticationBuilder AddClerkJwtBearer(
        this AuthenticationBuilder builder,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        // Add JWT Bearer authentication
        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            // Configure events for user provisioning
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    try
                    {
                        // Extract claims from JWT
                        var externalId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
                        var displayName = context.Principal?.FindFirst(ClaimTypes.Name)?.Value
                            ?? context.Principal?.FindFirst("name")?.Value;

                        if (string.IsNullOrEmpty(externalId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(displayName))
                        {
                            context.Fail("Missing required claims (sub, email, or name) in token.");
                            return;
                        }

                        // Upsert user in database (JIT provisioning)
                        var provisioningService = context.HttpContext.RequestServices
                            .GetRequiredService<IUserProvisioningService>();

                        var user = await provisioningService.UpsertUserAsync(
                            IdentityProvider.Clerk,
                            externalId,
                            email,
                            displayName,
                            context.HttpContext.RequestAborted);

                        // Enrich claims with database-backed data
                        var identity = (ClaimsIdentity)context.Principal!.Identity!;
                        identity.EnrichWithUserClaims(user);
                    }
                    catch (Exception ex)
                    {
                        context.Fail($"Error during user provisioning: {ex.Message}");
                    }
                }
            };

            // Allow additional configuration from caller
            configureOptions?.Invoke(options);
        });

        // Configure authority and audience from ClerkOptions
        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<ClerkOptions>>((jwtOptions, clerkOptions) =>
            {
                // Clerk uses OpenID Connect discovery with JWKS
                jwtOptions.Authority = clerkOptions.Value.Authority;

                // Set audience from configuration
                if (!string.IsNullOrEmpty(clerkOptions.Value.Audience))
                {
                    jwtOptions.Audience = clerkOptions.Value.Audience;
                    jwtOptions.TokenValidationParameters.ValidAudience = clerkOptions.Value.Audience;
                    jwtOptions.TokenValidationParameters.ValidateAudience = true;
                }
                else
                {
                    jwtOptions.TokenValidationParameters.ValidateAudience = false;
                }
            });

        return builder;
    }
}
