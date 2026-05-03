using System.Reflection;
using BuildingBlocks.Hosting.Enums;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Hosting.Extensions;

/// <summary>
/// Provides helpers for resolving the effective application execution mode from configuration or hosting context.
/// </summary>
public static class ApplicationExecutionModeExtensions
{
    private const string ExecutionModeConfigurationKey = "ExecutionMode";
    private const string OpenApiDocumentGeneratorEntryAssemblyName = "GetDocument.Insider";

    /// <summary>
    /// Resolves the execution mode explicitly from configuration, or falls back to OpenAPI build-time detection.
    /// </summary>
    public static ApplicationExecutionMode GetExecutionMode(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var configuredMode = builder.Configuration[ExecutionModeConfigurationKey];

        if (!string.IsNullOrWhiteSpace(configuredMode))
        {
            if (Enum.TryParse<ApplicationExecutionMode>(configuredMode, ignoreCase: true, out var executionMode))
                return executionMode;

            throw new InvalidOperationException(
                $"Unsupported execution mode '{configuredMode}'. Configure '{ExecutionModeConfigurationKey}' with one of: {string.Join(", ", Enum.GetNames<ApplicationExecutionMode>())}.");
        }

        if (Assembly.GetEntryAssembly()?.GetName().Name == OpenApiDocumentGeneratorEntryAssemblyName)
            return ApplicationExecutionMode.OpenApi;

        return ApplicationExecutionMode.Application;
    }
}