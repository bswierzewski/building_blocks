namespace BuildingBlocks.Hosting.Enums;

/// <summary>
/// Defines the high-level host execution mode used to adjust startup behavior for runtime, migrations, and OpenAPI generation.
/// </summary>
public enum ApplicationExecutionMode
{
    Application,
    OpenApi,
    Migration
}
