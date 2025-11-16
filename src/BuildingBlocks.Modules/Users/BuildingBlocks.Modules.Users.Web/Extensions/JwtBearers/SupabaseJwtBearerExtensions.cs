using System.Security.Claims;
using System.Text;
using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Application.Options;
using BuildingBlocks.Modules.Users.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Modules.Users.Web.Extensions.JwtBearers;

/// <summary>
/// Extension methods for adding Supabase JWT Bearer authentication.
/// </summary>
public static class SupabaseJwtBearerExtensions
{
    /// <summary>
    /// Adds Supabase JWT Bearer authentication with user provisioning.
    /// Configures JWT validation using HS256 symmetric key and enriches claims with user ID, roles, and permissions from database.
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="configureOptions">Optional additional JWT Bearer configuration</param>
    /// <returns>The authentication builder for chaining</returns>
    public static AuthenticationBuilder AddSupabaseJwtBearer(
        this AuthenticationBuilder builder,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        // Add JWT Bearer authentication
        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Configure events for user provisioning
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    try
                    {
                        // Extract claims from JWT
                        var externalId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? context.Principal?.FindFirstValue("sub");

                        var email = context.Principal?.FindFirstValue(ClaimTypes.Email);

                        if (string.IsNullOrEmpty(externalId) || string.IsNullOrEmpty(email))
                        {
                            context.Fail("Missing required claims (sub or email) in token.");
                            return;
                        }

                        // Get display name (fallback to email if not provided)
                        var displayName = context.Principal?.FindFirstValue("user_metadata.name")
                            ?? context.Principal?.FindFirstValue(ClaimTypes.Name)
                            ?? context.Principal?.FindFirstValue("name")
                            ?? context.Principal?.FindFirstValue("user_metadata.full_name")
                            ?? email;

                        // Upsert user in database (JIT provisioning)
                        var provisioningService = context.HttpContext.RequestServices
                            .GetRequiredService<IUserProvisioningService>();

                        var user = await provisioningService.UpsertUserAsync(
                            IdentityProvider.Supabase,
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

        // Configure HS256 signing key, issuer, and audience from SupabaseOptions
        builder.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<SupabaseOptions>>((jwtOptions, supabaseOptions) =>
            {
                // Create symmetric security key from JWT secret
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseOptions.Value.JwtSecret));
                jwtOptions.TokenValidationParameters.IssuerSigningKey = key;

                // Set issuer validation
                jwtOptions.TokenValidationParameters.ValidIssuer =
                    $"{supabaseOptions.Value.Authority}/auth/v1";

                // Set audience if provided
                if (!string.IsNullOrWhiteSpace(supabaseOptions.Value.Audience))
                {
                    jwtOptions.TokenValidationParameters.ValidAudience = supabaseOptions.Value.Audience;
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
