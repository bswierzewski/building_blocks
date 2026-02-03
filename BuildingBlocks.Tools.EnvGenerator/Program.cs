using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Get target directory from arguments or use current directory
var targetDirectory = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

if (!Directory.Exists(targetDirectory))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: Directory '{targetDirectory}' does not exist.");
    Console.ResetColor();
    return 1;
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Scanning directory: {targetDirectory}");
Console.ResetColor();

// Find all .cs files recursively
var csFiles = Directory.GetFiles(targetDirectory, "*.cs", SearchOption.AllDirectories);
Console.WriteLine($"Found {csFiles.Length} C# files");

// First pass: Build a dictionary of constants
Console.WriteLine("Building constants dictionary...");
var constants = new Dictionary<string, string>();

foreach (var filePath in csFiles)
{
    var sourceCode = await File.ReadAllTextAsync(filePath);
    var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
    var root = await syntaxTree.GetRootAsync();

    // Find field declarations with const or readonly modifiers from both classes and records
    var typeDeclarations = root.DescendantNodes().OfType<TypeDeclarationSyntax>();
    foreach (var typeDecl in typeDeclarations)
    {
        var typeName = typeDecl.Identifier.Text;
        var fieldDeclarations = typeDecl.Members.OfType<FieldDeclarationSyntax>()
            .Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword) || m.IsKind(SyntaxKind.ReadOnlyKeyword)));

        foreach (var field in fieldDeclarations)
        {
            foreach (var variable in field.Declaration.Variables)
            {
                if (variable.Initializer?.Value is LiteralExpressionSyntax literal)
                {
                    var key = $"{typeName}.{variable.Identifier.Text}";
                    var value = literal.Token.ValueText;
                    constants[key] = value;
                }
            }
        }
    }
}

Console.WriteLine($"Found {constants.Count} constant(s)");

// Second pass: Build a dictionary of all type definitions (classes and records) for nested type resolution
Console.WriteLine("Building type definitions dictionary...");
var allTypes = new Dictionary<string, TypeDeclarationSyntax>();

foreach (var filePath in csFiles)
{
    var sourceCode = await File.ReadAllTextAsync(filePath);
    var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
    var root = await syntaxTree.GetRootAsync();

    var typeDeclarations = root.DescendantNodes().OfType<TypeDeclarationSyntax>();
    foreach (var typeDecl in typeDeclarations)
    {
        var typeName = typeDecl.Identifier.Text;
        if (!allTypes.ContainsKey(typeName))
        {
            allTypes[typeName] = typeDecl;
        }
    }
}

Console.WriteLine($"Found {allTypes.Count} type definition(s)");

// Third pass: Parse files and find IOptions implementations
var optionsClasses = new List<OptionsClassInfo>();

foreach (var filePath in csFiles)
{
    var sourceCode = await File.ReadAllTextAsync(filePath);
    var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
    var root = await syntaxTree.GetRootAsync();

    // Find type declarations (classes and records) that implement IOptions
    var typeDeclarations = root.DescendantNodes()
        .OfType<TypeDeclarationSyntax>()
        .Where(t => t.BaseList?.Types.Any(bt => bt.ToString().Contains("IOptions")) == true);

    foreach (var typeDecl in typeDeclarations)
    {
        var optionsInfo = ExtractOptionsInfo(typeDecl, filePath, constants, allTypes);
        if (optionsInfo != null)
        {
            optionsClasses.Add(optionsInfo);

            // Log found IOptions class
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Found {optionsInfo.ClassName}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"    Section: {optionsInfo.SectionName}");
            Console.WriteLine($"    File: {Path.GetFileName(filePath)}");
            Console.WriteLine($"    Properties: {optionsInfo.Properties.Count}");
            Console.ResetColor();
        }
    }
}

if (optionsClasses.Count == 0)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("No IOptions implementations found.");
    Console.ResetColor();
    return 0;
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Found {optionsClasses.Count} IOptions implementation(s) total");
Console.ResetColor();

// Generate .env.example file
var envFilePath = Path.Combine(targetDirectory, ".env.example");
var envContent = GenerateEnvFile(optionsClasses, targetDirectory);

await File.WriteAllTextAsync(envFilePath, envContent);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"✓ Generated {envFilePath}");
Console.ResetColor();

return 0;

// Helper methods
static OptionsClassInfo? ExtractOptionsInfo(TypeDeclarationSyntax typeDecl, string filePath, Dictionary<string, string> constants, Dictionary<string, TypeDeclarationSyntax> allTypes)
{
    // Find SectionName property
    var sectionNameProperty = typeDecl.Members
        .OfType<PropertyDeclarationSyntax>()
        .FirstOrDefault(p => p.Identifier.Text == "SectionName" &&
                             p.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)));

    if (sectionNameProperty == null)
    {
        return null;
    }

    // Extract SectionName value
    var sectionName = ExtractSectionNameValue(sectionNameProperty, constants);
    if (string.IsNullOrEmpty(sectionName))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning: Could not extract SectionName from {typeDecl.Identifier.Text} in {filePath}");
        Console.ResetColor();
        return null;
    }

    // Extract public properties (with nested resolution)
    var properties = ExtractProperties(typeDecl, "", allTypes);

    return new OptionsClassInfo
    {
        ClassName = typeDecl.Identifier.Text,
        SectionName = sectionName,
        Properties = properties,
        FilePath = filePath
    };
}

static List<PropertyInfo> ExtractProperties(TypeDeclarationSyntax typeDecl, string prefix, Dictionary<string, TypeDeclarationSyntax> allTypes)
{
    var properties = new List<PropertyInfo>();

    foreach (var property in typeDecl.Members.OfType<PropertyDeclarationSyntax>())
    {
        // Skip static properties and SectionName
        if (property.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) ||
            property.Identifier.Text == "SectionName")
        {
            continue;
        }

        // Only include public properties with get/set
        if (!property.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
        {
            continue;
        }

        var propertyName = string.IsNullOrEmpty(prefix) ? property.Identifier.Text : $"{prefix}__{property.Identifier.Text}";
        var propertyType = property.Type.ToString();
        var defaultValue = ExtractDefaultValue(property);

        // Check if this is a complex type that we should expand
        if (!IsSimpleType(propertyType) && allTypes.TryGetValue(propertyType, out var nestedType))
        {
            // Recursively extract nested properties
            var nestedProperties = ExtractProperties(nestedType, propertyName, allTypes);
            properties.AddRange(nestedProperties);
        }
        else
        {
            // Add simple property
            var propertyInfo = new PropertyInfo
            {
                Name = propertyName,
                Type = propertyType,
                DefaultValue = defaultValue
            };

            properties.Add(propertyInfo);
        }
    }

    return properties;
}

static string? ExtractSectionNameValue(PropertyDeclarationSyntax property, Dictionary<string, string> constants)
{
    // Look for arrow expression body: => "value"
    if (property.ExpressionBody?.Expression is InterpolatedStringExpressionSyntax interpolated)
    {
        // Handle string interpolation like $"Modules:{ModuleName}:Service"
        var result = new StringBuilder();
        foreach (var content in interpolated.Contents)
        {
            if (content is InterpolatedStringTextSyntax text)
            {
                result.Append(text.TextToken.Text);
            }
            else if (content is InterpolationSyntax interpolation)
            {
                var expression = interpolation.Expression.ToString();

                // Try to resolve from constants dictionary
                if (constants.TryGetValue(expression, out var constantValue))
                {
                    result.Append(constantValue);
                }
                else
                {
                    // Fallback to placeholder
                    result.Append($"{{{expression}}}");
                }
            }
        }
        return result.ToString();
    }
    else if (property.ExpressionBody?.Expression is LiteralExpressionSyntax literal)
    {
        return literal.Token.ValueText;
    }

    return null;
}

static string? ExtractDefaultValue(PropertyDeclarationSyntax property)
{
    // Look for initializer: = value;
    if (property.Initializer?.Value is LiteralExpressionSyntax literal)
    {
        // For string.Empty or similar
        return literal.Token.ValueText;
    }
    else if (property.Initializer?.Value is MemberAccessExpressionSyntax memberAccess)
    {
        // Handle string.Empty
        if (memberAccess.ToString() == "string.Empty")
        {
            return "";
        }
    }
    else if (property.Initializer?.Value is ObjectCreationExpressionSyntax)
    {
        // Skip complex object initializations
        return null;
    }

    return null;
}

static string GenerateEnvFile(List<OptionsClassInfo> optionsClasses, string baseDirectory)
{
    var sb = new StringBuilder();
    sb.AppendLine("# Auto-generated .env.example file");
    sb.AppendLine($"# Generated on {DateTime.Now:yyyy-MM-dd}");
    sb.AppendLine();

    // Group by top-level section for better organization
    var grouped = optionsClasses
        .GroupBy(o => o.SectionName.Split(':')[0])
        .OrderBy(g => g.Key);

    foreach (var group in grouped)
    {
        foreach (var optionsClass in group.OrderBy(o => o.SectionName))
        {
            var fileName = Path.GetFileName(optionsClass.FilePath);

            sb.AppendLine("# ==========================================");
            sb.AppendLine($"# {optionsClass.SectionName}");
            sb.AppendLine($"# File: {fileName}");
            sb.AppendLine("# ==========================================");

            // Convert SectionName to env format: "Modules:Orders:Products" -> "Modules__Orders__Products__"
            var sectionPrefix = optionsClass.SectionName.Replace(":", "__").ToUpperInvariant() + "__";

            foreach (var property in optionsClass.Properties.OrderBy(p => p.Name))
            {
                sb.AppendLine($"# Type: {property.Type}");
                var envKey = sectionPrefix + property.Name.Replace("__", "__").ToUpperInvariant();
                var envValue = property.DefaultValue ?? "";
                sb.AppendLine($"{envKey}={envValue}");
                sb.AppendLine();
            }
        }
    }

    return sb.ToString();
}

static bool IsSimpleType(string type)
{
    var simpleTypes = new[] { "string", "int", "bool", "double", "float", "decimal", "long", "DateTime", "Guid" };
    return simpleTypes.Any(t => type.Contains(t));
}

// Data classes
class OptionsClassInfo
{
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public List<PropertyInfo> Properties { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
}

class PropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
}
