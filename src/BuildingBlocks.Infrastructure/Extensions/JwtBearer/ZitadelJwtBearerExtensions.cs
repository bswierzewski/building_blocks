using BuildingBlocks.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Extensions.JwtBearer;

public static class ZitadelJwtBearerExtensions
{
    extension(AuthenticationBuilder builder)
    {
        public AuthenticationBuilder AddZitadelJwtBearer()
        {
            builder.Services.AddOptions<ZitadelOptions>()
                .BindConfiguration(ZitadelOptions.SectionName)
                .ValidateOnStart();

            builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<ZitadelOptions>>((jwt, config) =>
                {
                    var options = config.Value;

                    jwt.Authority = options.Authority;
                    jwt.Audience = options.Audience;
                    jwt.RequireHttpsMetadata = false;
                    jwt.TokenValidationParameters.NameClaimType = ClaimTypes.Name;
                    jwt.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

                    jwt.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = ctx => HandleOnTokenValidated(ctx, options)
                    };
                });

            return builder.AddJwtBearer();
        }
    }

    private static Task HandleOnTokenValidated(TokenValidatedContext context, ZitadelOptions options)
    {
        if (context.Principal?.Identity is not ClaimsIdentity identity)
            return Task.CompletedTask;

        var roleClaim = identity.FindFirst("urn:zitadel:iam:org:project:roles");

        if (roleClaim is not null && !string.IsNullOrEmpty(roleClaim.Value))
        {
            try
            {
                using var doc = JsonDocument.Parse(roleClaim.Value);

                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, prop.Name));
                    }
                }
            }
            catch
            {
            }
        }

        return Task.CompletedTask;
    }
}