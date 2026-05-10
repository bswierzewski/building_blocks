using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BuildingBlocks.Infrastructure.OpenApi;

/// <summary>
/// Provides extension methods for configuring OpenAPI options to include standardized RFC 7807 Problem Details error
/// responses and schema enhancements.
/// </summary>
public static class OpenApiExtensions
{
    private const string ProblemDetailsSchemaName = nameof(HttpValidationProblemDetails);
    private const string ProblemJsonContentType = "application/problem+json";
    private const string DefaultErrorResponseDescription = "An error occurred";

    public static OpenApiOptions AddProblemDetailsResponses(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddOperationTransformer(async (operation, context, cancellationToken) =>
        {
            operation.Responses ??= [];

            if (operation.Responses.ContainsKey("default"))
                return;

            IOpenApiSchema problemDetailsSchema;

            if (context.Document is null)
            {
                problemDetailsSchema = await context.GetOrCreateSchemaAsync(
                    typeof(HttpValidationProblemDetails),
                    cancellationToken: cancellationToken);
            }
            else
            {
                context.Document.Components ??= new OpenApiComponents();
                context.Document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

                if (!context.Document.Components.Schemas.ContainsKey(ProblemDetailsSchemaName))
                {
                    context.Document.AddComponent(
                        ProblemDetailsSchemaName,
                        await context.GetOrCreateSchemaAsync(
                            typeof(HttpValidationProblemDetails),
                            cancellationToken: cancellationToken));
                }

                problemDetailsSchema = new OpenApiSchemaReference(ProblemDetailsSchemaName, context.Document);
            }

            operation.Responses["default"] = new OpenApiResponse
            {
                Description = DefaultErrorResponseDescription,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [ProblemJsonContentType] = new OpenApiMediaType
                    {
                        Schema = problemDetailsSchema
                    }
                }
            };
        });

        return options;
    }

    public static OpenApiOptions AddBearerSecurityScheme(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste your Clerk JWT token (without the 'Bearer ' prefix)"
            };

            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            var hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<AuthorizeAttribute>()
                .Any();

            if (!hasAuthorize)
                return Task.CompletedTask;

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });

            return Task.CompletedTask;
        });

        return options;
    }
}