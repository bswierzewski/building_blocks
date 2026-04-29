using BuildingBlocks.Infrastructure.Middleware;
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
    private const string ProblemDetailsSchemaName = "ProblemDetails";
    private const string ProblemJsonContentType = "application/problem+json";
    private const string DefaultErrorResponseDescription = "An error occurred";

    public static OpenApiOptions AddProblemDetailsResponses(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(ProblemDetails))
                return Task.CompletedTask;

            schema.Description ??= "RFC 7807 Problem Details for HTTP APIs";
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();

            schema.Properties.TryAdd("traceId", new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "The trace identifier for request tracking."
            });

            schema.Properties.TryAdd("timestamp", new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "date-time",
                Description = "Timestamp when the error occurred (UTC)."
            });

            schema.Properties.TryAdd("errors", new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "Validation errors grouped by field name.",
                AdditionalProperties = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    }
                }
            });

            return Task.CompletedTask;
        });

        options.AddOperationTransformer(async (operation, context, cancellationToken) =>
        {
            operation.Responses ??= [];

            if (operation.Responses.ContainsKey("default"))
                return;

            var problemDetailsSchema = await GetProblemDetailsSchemaAsync(context, cancellationToken);

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

    private static async Task<IOpenApiSchema> GetProblemDetailsSchemaAsync(OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Document is null)
            return await context.GetOrCreateSchemaAsync(typeof(ProblemDetails), cancellationToken: cancellationToken);

        context.Document.Components ??= new OpenApiComponents();
        context.Document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        if (!context.Document.Components.Schemas.ContainsKey(ProblemDetailsSchemaName))
        {
            context.Document.AddComponent(
                ProblemDetailsSchemaName,
                await context.GetOrCreateSchemaAsync(typeof(ProblemDetails), cancellationToken: cancellationToken));
        }

        return new OpenApiSchemaReference(ProblemDetailsSchemaName, context.Document);
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