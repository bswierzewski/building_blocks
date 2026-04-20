namespace BuildingBlocks.Core.Primitives;

/// <summary>
/// Describes a permission published by a module.
/// </summary>
/// <param name="Code">Stable unique permission code, for example orders:read.</param>
/// <param name="Description">Business-facing description used by administration surfaces.</param>
public sealed record Permission(string Code, string Description);