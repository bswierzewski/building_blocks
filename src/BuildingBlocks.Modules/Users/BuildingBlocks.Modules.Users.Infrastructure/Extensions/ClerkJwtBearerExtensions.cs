using System.Security.Claims;
using System.Xml.Linq;
using BuildingBlocks.Application.Security;
using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.Enums;
using BuildingBlocks.Modules.Users.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Modules.Users.Infrastructure.Extensions;

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
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Configure events for user provisioning
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    try
                    {
                        // 1. Extract data from JWT token (already in ClaimsPrincipal)
                        // JWT Bearer middleware automatically maps JWT 'sub' claim to ClaimTypes.NameIdentifier (external ID)
                        var externalId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (string.IsNullOrEmpty(externalId))
                        {
                            context.Fail("Missing 'NameIdentifier' claim in token.");
                            return;
                        }

                        var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
                        if (string.IsNullOrEmpty(email))
                        {
                            context.Fail("Missing 'Email' claim in token.");
                            return;
                        }

                        var displayName = context.Principal?.FindFirst(ClaimTypes.Name)?.Value
                            ?? context.Principal?.FindFirst("name")?.Value;
                        if (string.IsNullOrEmpty(displayName))
                        {
                            context.Fail("Missing 'Name' claim in token.");
                            return;
                        }

                        // Get identity for claim manipulation
                        var identity = (ClaimsIdentity)context.Principal!.Identity!;

                        // 2. Get or create user in database
                        // At this point, IUser.Id returns null because CustomClaimTypes.UserId hasn't been added yet
                        // This allows AuditableEntityInterceptor to set CreatedBy to null during JIT provisioning
                        var provisioningService = context.HttpContext.RequestServices
                            .GetRequiredService<IUserProvisioningService>();

                        var user = await provisioningService.GetUserAsync(
                            provider: IdentityProvider.Clerk,
                            externalUserId: externalId,
                            cancellationToken: context.HttpContext.RequestAborted
                        );

                        if (user == null)
                        {
                            // JIT provisioning: Create new user
                            // AuditableEntityInterceptor will set CreatedBy to null (system-created)
                            user = await provisioningService.AddUserAsync(
                                provider: IdentityProvider.Clerk,
                                externalUserId: externalId,
                                email: email,
                                displayName: displayName,
                                cancellationToken: context.HttpContext.RequestAborted
                            );
                        }
                        else
                        {
                            // Update existing user if profile data changed
                            if (displayName != user.DisplayName)
                            {
                                await provisioningService.UpdateUserAsync(
                                    user: user,
                                    displayName: displayName,
                                    cancellationToken: context.HttpContext.RequestAborted
                                );
                            }
                        }

                        // 3. Enrich claims with database-backed data

                        // Add internal user ID (GUID from database) as custom claim
                        // ClaimTypes.NameIdentifier remains unchanged with external ID from JWT 'sub' claim
                        identity.AddClaim(new Claim(CustomClaimTypes.UserId, user.Id.Value.ToString()));

                        // Add roles from database (using standard ClaimTypes.Role)
                        foreach (var role in user.Roles)
                            identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));

                        // Add permissions from database (using custom permission claim)
                        foreach (var permission in user.GetAllPermissions())
                            identity.AddClaim(new Claim(CustomClaimTypes.Permission, permission.Name));
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

        // Configure ClerkOptions-based settings
        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<ClerkOptions>>((jwtOptions, clerkOptions) =>
            {
                jwtOptions.Authority = clerkOptions.Value.Authority;
                jwtOptions.Audience = clerkOptions.Value.Audience;

                // Update audience validation based on whether Audience is set
                if (jwtOptions.TokenValidationParameters != null)
                {
                    jwtOptions.TokenValidationParameters.ValidateAudience = !string.IsNullOrEmpty(clerkOptions.Value.Audience);
                }
            });

        return builder;
    }
}
