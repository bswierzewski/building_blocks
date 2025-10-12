using System.Security.Claims;
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
                    var externalId = context.Principal?.FindFirst("sub")?.Value;
                    if (string.IsNullOrEmpty(externalId))
                    {
                        context.Fail("Missing 'sub' claim in JWT token");
                        return;
                    }

                    // Get provisioning service
                    var provisioningService = context.HttpContext.RequestServices
                        .GetRequiredService<IUserProvisioningService>();

                    try
                    {
                        // Try to get existing user
                        var user = await provisioningService.GetUserAsync(
                            provider: IdentityProvider.Clerk,
                            externalUserId: externalId,
                            cancellationToken: context.HttpContext.RequestAborted
                        );

                        // If user doesn't exist, create new one (JIT Provisioning)
                        if (user == null)
                        {
                            var email = context.Principal?.FindFirst("email")?.Value;
                            var name = context.Principal?.FindFirst("name")?.Value;

                            user = await provisioningService.AddUserAsync(
                                provider: IdentityProvider.Clerk,
                                externalUserId: externalId,
                                email: email,
                                displayName: name,
                                cancellationToken: context.HttpContext.RequestAborted
                            );
                        }

                        // Map JWT claims to standard ClaimTypes
                        var identity = (ClaimsIdentity)context.Principal!.Identity!;

                        // Map email to standard ClaimTypes.Email
                        var emailClaim = context.Principal?.FindFirst("email")?.Value;
                        if (!string.IsNullOrEmpty(emailClaim))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim));
                        }

                        // Map name to standard ClaimTypes.Name
                        var nameClaim = context.Principal?.FindFirst("name")?.Value;
                        if (!string.IsNullOrEmpty(nameClaim))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim));
                        }

                        // Map picture URL (keep as "picture" - not part of standard ClaimTypes)
                        var pictureClaim = context.Principal?.FindFirst("picture")?.Value;
                        if (!string.IsNullOrEmpty(pictureClaim))
                        {
                            identity.AddClaim(new Claim("picture", pictureClaim));
                        }

                        // Add internal user ID
                        identity.AddClaim(new Claim("user_id", user.Id.ToString()));

                        // Add roles from database (using standard ClaimTypes.Role)
                        foreach (var role in user.Roles)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
                        }

                        // Add permissions from database
                        foreach (var permission in user.GetAllPermissions())
                        {
                            identity.AddClaim(new Claim("permission", permission.Name));
                        }
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
