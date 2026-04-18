namespace BuildingBlocks.Core.Extensions;

/// <summary>
/// Provides naming helpers for module identifiers and database schema names.
/// </summary>
public static class ModuleExtensions
{
  private const string DbContextSuffix = "DbContext";

  /// <summary>
  /// Converts a DbContext type name into the conventional database schema name.
  /// </summary>
  public static string ToDbContextSchemaName(this string dbContextName)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(dbContextName);

    var normalizedName = dbContextName.EndsWith(DbContextSuffix, StringComparison.Ordinal)
        ? dbContextName[..^DbContextSuffix.Length]
        : dbContextName;

    return normalizedName.ToLowerInvariant();
  }

  /// <summary>
  /// Converts a DbContext type into the conventional database schema name.
  /// </summary>
  public static string ToDbContextSchemaName(this Type dbContextType)
  {
    ArgumentNullException.ThrowIfNull(dbContextType);

    return dbContextType.Name.ToDbContextSchemaName();
  }
}