using System.Reflection;
using System.Text;
using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Attributes;

namespace BuildingBlocks.Tools.Services;

/// <summary>
/// Service responsible for scanning .NET assemblies and generating .env files.
///
/// Features:
/// - Scans assemblies for classes implementing IOptions interface
/// - Scans for classes with EnvSectionAttribute
/// - Recursively follows assembly references to find all configuration classes
/// - Generates environment variables in SECTION__PROPERTY_NAME format
/// - Supports sensitive value marking, descriptions, and default values
/// - Deduplicates types to avoid duplicate sections
/// </summary>
public class EnvFileGenerator
{
    /// <summary>
    /// Generates an .env file by scanning the provided assemblies.
    /// </summary>
    /// <param name="assemblyPaths">Paths to assemblies to scan</param>
    /// <param name="outputPath">Output .env file path</param>
    /// <param name="recursive">Whether to scan referenced assemblies</param>
    /// <param name="includeDescriptions">Include comments with descriptions</param>
    /// <param name="overwrite">Overwrite existing file without prompting</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task GenerateAsync(
        string[] assemblyPaths,
        string outputPath,
        bool recursive,
        bool includeDescriptions,
        bool overwrite,
        CancellationToken cancellationToken = default)
    {
        if (File.Exists(outputPath) && !overwrite)
        {
            Console.WriteLine($"File {outputPath} already exists. Use --force to overwrite.");
            return;
        }

        Console.WriteLine("Scanning assemblies for IOptions implementations...");

        var optionsTypes = new List<Type>();

        foreach (var assemblyPath in assemblyPaths)
        {
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine($"Warning: Assembly not found: {assemblyPath}");
                continue;
            }

            var types = ScanAssembly(assemblyPath, recursive);
            optionsTypes.AddRange(types);
        }

        if (optionsTypes.Count == 0)
        {
            Console.WriteLine("No IOptions implementations found.");
            return;
        }

        Console.WriteLine($"Found {optionsTypes.Count} configuration section(s).");

        var content = GenerateEnvContent(optionsTypes, includeDescriptions);
        await File.WriteAllTextAsync(outputPath, content, cancellationToken);

        Console.WriteLine($"Generated {outputPath} successfully.");
    }

    /// <summary>
    /// Scans a single assembly and optionally its references for IOptions implementations.
    /// </summary>
    private List<Type> ScanAssembly(string assemblyPath, bool recursive)
    {
        var result = new List<Type>();
        var scannedAssemblies = new HashSet<string>();

        ScanAssemblyRecursive(assemblyPath, recursive, result, scannedAssemblies);

        return result;
    }

    /// <summary>
    /// Recursively scans assemblies, tracking already-scanned assemblies to avoid duplicates.
    /// Sets up assembly resolution to load dependencies from the same directory.
    /// Handles ReflectionTypeLoadException for assemblies with missing dependencies.
    /// </summary>
    private void ScanAssemblyRecursive(
        string assemblyPath,
        bool recursive,
        List<Type> result,
        HashSet<string> scannedAssemblies)
    {
        var fullPath = Path.GetFullPath(assemblyPath);

        if (scannedAssemblies.Contains(fullPath))
            return;

        scannedAssemblies.Add(fullPath);

        try
        {
            // Set up assembly resolution for dependencies
            var assemblyDir = Path.GetDirectoryName(fullPath)!;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var assemblyName = new AssemblyName(args.Name).Name;
                var dependencyPath = Path.Combine(assemblyDir, assemblyName + ".dll");
                if (File.Exists(dependencyPath))
                {
                    return Assembly.LoadFrom(dependencyPath);
                }
                return null;
            };

            var assembly = Assembly.LoadFrom(fullPath);
            Console.WriteLine($"  Scanning: {assembly.GetName().Name}");

            // Find types implementing IOptions - handle ReflectionTypeLoadException
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Get the types that were successfully loaded
                types = ex.Types.Where(t => t != null).ToArray()!;
                Console.WriteLine($"    Warning: Some types could not be loaded, scanning {types.Length} available types");
            }

            var optionsTypes = types
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
                            t.GetInterfaces().Any(i => i == typeof(IOptions)));

            foreach (var type in optionsTypes)
            {
                Console.WriteLine($"    Found: {type.Name}");
                result.Add(type);
            }

            // Also find types with EnvSectionAttribute
            var attributeTypes = types
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
                            t.GetCustomAttribute<EnvSectionAttribute>() != null &&
                            !result.Contains(t));

            foreach (var type in attributeTypes)
            {
                Console.WriteLine($"    Found (attribute): {type.Name}");
                result.Add(type);
            }

            // Recursively scan referenced assemblies
            if (recursive)
            {
                foreach (var reference in assembly.GetReferencedAssemblies())
                {
                    var refPath = Path.Combine(assemblyDir, reference.Name + ".dll");
                    if (File.Exists(refPath))
                    {
                        ScanAssemblyRecursive(refPath, recursive, result, scannedAssemblies);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Could not load {assemblyPath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates the .env file content from discovered IOptions types.
    /// Deduplicates by full type name, sorts by section name, and formats each property
    /// as SECTION__PROPERTY_NAME with optional comments and metadata.
    /// </summary>
    private string GenerateEnvContent(List<Type> optionsTypes, bool includeDescriptions)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Environment Variables Configuration");
        sb.AppendLine($"# Generated by BuildingBlocks.Tools on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Deduplicate by full type name to avoid duplicates from recursive scanning
        var uniqueTypes = optionsTypes
            .GroupBy(t => t.FullName)
            .Select(g => g.First())
            .ToList();

        foreach (var type in uniqueTypes.OrderBy(t => GetSectionName(t)))
        {
            var sectionName = GetSectionName(type);
            var sectionDescription = GetSectionDescription(type);

            sb.AppendLine($"# ==========================================");
            sb.AppendLine($"# {sectionName}");

            if (includeDescriptions && !string.IsNullOrEmpty(sectionDescription))
            {
                sb.AppendLine($"# {sectionDescription}");
            }

            sb.AppendLine($"# ==========================================");

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .OrderBy(p => p.Name);

            foreach (var property in properties)
            {
                var envAttr = property.GetCustomAttribute<EnvVariableAttribute>();
                var envName = GetEnvVariableName(sectionName, property, envAttr);
                var defaultValue = envAttr?.DefaultValue ?? GetDefaultValue(property.PropertyType);
                var description = envAttr?.Description;
                var required = envAttr?.Required ?? true;
                var sensitive = envAttr?.Sensitive ?? false;

                if (includeDescriptions)
                {
                    if (!string.IsNullOrEmpty(description))
                    {
                        sb.AppendLine($"# {description}");
                    }

                    var metadata = new List<string>();
                    if (required) metadata.Add("Required");
                    if (sensitive) metadata.Add("Sensitive");
                    metadata.Add($"Type: {GetTypeName(property.PropertyType)}");

                    sb.AppendLine($"# [{string.Join(", ", metadata)}]");
                }

                if (sensitive)
                {
                    sb.AppendLine($"{envName}=<SENSITIVE_VALUE>");
                }
                else
                {
                    sb.AppendLine($"{envName}={defaultValue}");
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the configuration section name from a type.
    /// Priority: EnvSectionAttribute > static SectionName property > class name without "Options" suffix
    /// </summary>
    private string GetSectionName(Type type)
    {
        // Check for EnvSectionAttribute first
        var attr = type.GetCustomAttribute<EnvSectionAttribute>();
        if (attr != null)
            return attr.SectionName;

        // Check for static SectionName property (IOptions interface)
        var sectionNameProperty = type.GetProperty("SectionName", BindingFlags.Public | BindingFlags.Static);
        if (sectionNameProperty != null)
        {
            return sectionNameProperty.GetValue(null)?.ToString() ?? type.Name.Replace("Options", "");
        }

        return type.Name.Replace("Options", "");
    }

    private string? GetSectionDescription(Type type)
    {
        var attr = type.GetCustomAttribute<EnvSectionAttribute>();
        return attr?.Description;
    }

    /// <summary>
    /// Gets the environment variable name for a property.
    /// Uses custom name from EnvVariableAttribute if specified,
    /// otherwise generates SECTIONNAME__PROPERTYNAME format (uppercase without word separators).
    /// This matches .NET configuration's environment variable naming convention.
    /// </summary>
    private string GetEnvVariableName(string sectionName, PropertyInfo property, EnvVariableAttribute? attr)
    {
        if (!string.IsNullOrEmpty(attr?.Name))
            return attr.Name;

        // Convert to uppercase without adding underscores between words
        // This matches .NET configuration binding expectations
        // Example: "SnippetsDatabase" + "ConnectionString" -> "SNIPPETSDATABASE__CONNECTIONSTRING"
        var propertyName = property.Name.ToUpperInvariant();
        var section = sectionName.ToUpperInvariant();

        // Use double underscore as section separator (standard .NET configuration pattern)
        return $"{section}__{propertyName}";
    }

    /// <summary>
    /// Converts PascalCase or camelCase to SCREAMING_SNAKE_CASE.
    /// Example: "ConnectionString" -> "CONNECTION_STRING"
    /// </summary>
    private string ToScreamingSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (char.IsUpper(c) && i > 0)
            {
                var prevChar = input[i - 1];
                if (!char.IsUpper(prevChar) && prevChar != '_')
                {
                    result.Append('_');
                }
            }

            result.Append(char.ToUpperInvariant(c));
        }

        return result.ToString();
    }

    private string GetDefaultValue(Type type)
    {
        if (type == typeof(string))
            return "";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short))
            return "0";
        if (type == typeof(bool))
            return "false";
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal))
            return "0.0";
        if (type == typeof(Guid))
            return "00000000-0000-0000-0000-000000000000";
        if (type == typeof(TimeSpan))
            return "00:00:00";
        if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            return "";

        return "";
    }

    private string GetTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(short)) return "short";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(Guid)) return "Guid";
        if (type == typeof(TimeSpan)) return "TimeSpan";
        if (type.IsArray) return $"{GetTypeName(type.GetElementType()!)}[]";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return $"List<{GetTypeName(type.GetGenericArguments()[0])}>";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return $"{GetTypeName(type.GetGenericArguments()[0])}?";

        return type.Name;
    }
}
