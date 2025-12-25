using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                // Add timestamp to all Problem Details
                context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;

                // In Development: Add exception details for debugging
                if (environment.IsDevelopment() && context.Exception is not null)
                {
                    context.ProblemDetails.Extensions["exceptionType"] = context.Exception.GetType().Name;
                    context.ProblemDetails.Extensions["exceptionMessage"] = context.Exception.Message;
                    context.ProblemDetails.Extensions["stackTrace"] = context.Exception.StackTrace;
                }
            };

            return options;
        }
    }

    extension(OpenApiOptions options)
    {
        public OpenApiOptions AddProblemDetailsSchemas()
        {
            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                if (context.JsonTypeInfo.Type != typeof(ProblemDetails) &&
                    !context.JsonTypeInfo.Type.IsSubclassOf(typeof(ProblemDetails)))
                {
                    return Task.CompletedTask;
                }

                schema.Properties ??= new Dictionary<string, OpenApiSchema>();

                if (!schema.Properties.ContainsKey("traceId"))
                {
                    schema.Properties["traceId"] = new OpenApiSchema
                    {
                        Type = "string",
                        Description = "The trace identifier for request tracking and debugging.",
                        ReadOnly = true
                    };
                }

                if (!schema.Properties.ContainsKey("timestamp"))
                {
                    schema.Properties["timestamp"] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "date-time", // Format ISO 8601
                        Description = "The timestamp when the error occurred (UTC).",
                        ReadOnly = true
                    };
                }

                if (!schema.Properties.ContainsKey("errors"))
                {
                    schema.Properties["errors"] = new OpenApiSchema
                    {
                        Type = "object",
                        Description = "Validation errors grouped by field name.",
                        // Definicja Dictionary<string, string[]> w OpenAPI:
                        AdditionalProperties = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Type = "string" }
                        }
                    };
                }

                return Task.CompletedTask;
            });

            return options;
        }
    }
}