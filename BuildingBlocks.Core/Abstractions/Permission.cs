namespace BuildingBlocks.Core.Abstractions;

/// <summary>
/// Descriptor of a permission defined by a module.
/// Equality is based solely on <see cref="Code"/> (case-insensitive).
/// </summary>
/// <param name="Code">Unique permission code in <c>resource:action</c> format, e.g. <c>users:read</c>.</param>
/// <param name="Description">Human-readable description of the permission.</param>
public sealed record Permission(string Code, string Description);