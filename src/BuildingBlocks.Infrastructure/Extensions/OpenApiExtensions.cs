using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class OpenApiExtensions
{
    extension(ProblemDetailsOptions options)
    {
        public ProblemDetailsOptions AddCustomConfiguration(IHostEnvironment environment)
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
            };

            return options;
        }
    }

    extension(OpenApiOptions options)
    {
        public OpenApiOptions AddProblemDetailsSchemas()
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                // Ensure components exist
                document.Components ??= new OpenApiComponents();
                document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

                // Add ProblemDetails schema
                document.Components.Schemas["ProblemDetails"] = CreateProblemDetailsSchema();

                // Add default error response to all operations for client generators (Orval, etc.)
                AddDefaultErrorResponse(document);

                return Task.CompletedTask;
            });

            return options;
        }
    }

    private static OpenApiSchema CreateProblemDetailsSchema() => new()
    {
        Type = "object",
        Description = "RFC 7807 Problem Details for HTTP APIs",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["type"] = new() { Type = "string", Description = "A URI reference that identifies the problem type." },
            ["title"] = new() { Type = "string", Description = "A short, human-readable summary of the problem type." },
            ["status"] = new() { Type = "integer", Format = "int32", Description = "The HTTP status code." },
            ["detail"] = new() { Type = "string", Description = "A human-readable explanation specific to this occurrence." },
            ["instance"] = new() { Type = "string", Description = "A URI reference that identifies the specific occurrence." },
            ["traceId"] = new() { Type = "string", Description = "The trace identifier for request tracking." },
            ["timestamp"] = new() { Type = "string", Format = "date-time", Description = "The timestamp when the error occurred (UTC)." },
            ["errors"] = new()
            {
                Type = "object",
                Description = "Validation errors grouped by field name.",
                AdditionalProperties = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "string" }
                }
            }
        }
    };

    private static void AddDefaultErrorResponse(OpenApiDocument document)
    {
        if (document.Paths == null) return;

        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                operation.Responses ??= [];

                // Add only "default" response (represents any undefined error code)
                // This is minimal and tells client generators (like Orval) that errors are ProblemDetails
                if (operation.Responses.ContainsKey("default"))
                    continue;

                operation.Responses["default"] = new OpenApiResponse
                {
                    Description = "Error response",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "ProblemDetails"
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}