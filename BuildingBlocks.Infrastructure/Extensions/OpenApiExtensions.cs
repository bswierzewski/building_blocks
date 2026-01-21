using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class OpenApiProblemDetailsExtensions
{
    public static OpenApiOptions AddProblemDetailsResponses(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

            if (!document.Components.Schemas.ContainsKey("ProblemDetails"))            
                document.Components.Schemas["ProblemDetails"] = CreateProblemDetailsSchema();            

            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            if (!operation.Responses.ContainsKey("default"))
            {
                operation.Responses.Add("default", new OpenApiResponse
                {
                    Description = "An error occurred",
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
                });
            }

            return Task.CompletedTask;
        });

        return options;
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
}
