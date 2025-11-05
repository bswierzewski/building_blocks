using System.Security.Claims;
using BuildingBlocks.Application.Security;
using BuildingBlocks.Modules.Users.Application.Abstractions;
using BuildingBlocks.Modules.Users.Domain.Enums;
using BuildingBlocks.Modules.Users.Web.Options;
using BuildingBlocks.Modules.Users.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Modules.Users.Web.Extensions.JwtBearers;

/// <summary>
/// Extension methods for adding Supabase JWT Bearer authentication.
/// </summary>
public static class SupabaseJwtBearerExtensions
{
    /// <summary>
    /// Adds Supabase JWT Bearer authentication with user provisioning.
    /// Configures JWT validation using JWKS (JSON Web Key Set) and enriches claims with user ID, roles, and permissions from database.
    /// Uses modern asymmetric key validation (ES256) with automatic key rotation support.
    /// </summary>
    /// <param name="builder">The authentication builder</param>
    /// <param name="configureOptions">Optional additional JWT Bearer configuration</param>
    /// <returns>The authentication builder for chaining</returns>
    public static AuthenticationBuilder AddSupabaseJwtBearer(
        this AuthenticationBuilder builder,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        // Register named HttpClient for JWKS endpoint with appropriate timeouts
        builder.Services.AddHttpClient(nameof(SupabaseSigningKeyResolver), client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Register ConfigurationManager for JWKS as singleton
        // DI container will automatically dispose it on app shutdown
        builder.Services.AddSingleton<IConfigurationManager<JsonWebKeySet>>(serviceProvider =>
        {
            var supabaseOptions = serviceProvider.GetRequiredService<IOptions<SupabaseOptions>>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(SupabaseSigningKeyResolver));

            return new ConfigurationManager<JsonWebKeySet>(
                supabaseOptions.Value.JwksUrl,
                new JwksConfigurationRetriever(),
                new JwksDocumentRetriever(httpClient));
        });

        // Register signing key resolver as singleton
        builder.Services.AddSingleton<SupabaseSigningKeyResolver>();

        // Add JWT Bearer authentication
        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false, // Supabase doesn't set issuer in JWT
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
                        var externalId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;

                        if (string.IsNullOrEmpty(externalId) || string.IsNullOrEmpty(email))
                        {
                            context.Fail("Missing required claims (sub or email) in token.");
                            return;
                        }

                        // Get display name (fallback to email if not provided)
                        var displayName = context.Principal?.FindFirst(ClaimTypes.Name)?.Value
                            ?? context.Principal?.FindFirst("name")?.Value
                            ?? context.Principal?.FindFirst("user_metadata.name")?.Value
                            ?? context.Principal?.FindFirst("user_metadata.full_name")?.Value
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

        // Configure JWKS resolver and audience from SupabaseOptions
        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<SupabaseOptions>, IServiceProvider>((jwtOptions, supabaseOptions, serviceProvider) =>
            {
                var keyResolver = serviceProvider.GetRequiredService<SupabaseSigningKeyResolver>();

                // Use key resolver with ConfigurationManager caching and automatic refresh
                jwtOptions.TokenValidationParameters.IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                {
                    return keyResolver.GetSigningKeys();
                };

                // Set audience from configuration
                if (!string.IsNullOrEmpty(supabaseOptions.Value.Audience))
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
