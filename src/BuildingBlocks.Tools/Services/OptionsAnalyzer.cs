using BuildingBlocks.Abstractions.Abstractions;
using BuildingBlocks.Tools.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BuildingBlocks.Tools.Services;

/// <summary>
/// Analyzes code to find and extract IOptions implementations.
/// </summary>
internal class OptionsAnalyzer
{
    private static readonly string TargetInterfaceName = typeof(IOptions).FullName!;
    private static readonly string SectionNameProperty = nameof(IOptions.SectionName);

    /// <summary>
    /// Finds all IOptions implementations in a solution.
    /// </summary>
    /// <param name="solution">The solution to analyze.</param>
    /// <returns>List of options classes found.</returns>
    public async Task<List<OptionsClassInfo>> AnalyzeAsync(Solution solution)
    {
        var optionsClasses = new List<OptionsClassInfo>();

        foreach (var project in solution.Projects)
        {
            Console.WriteLine($"Scanning project: {project.Name}...");
            var compilation = await project.GetCompilationAsync();

            if (compilation == null)
            {
                Console.WriteLine($"  Warning: Could not get compilation for {project.Name}");
                continue;
            }

            // Get the IOptions interface symbol
            var iOptionsSymbol = compilation.GetTypeByMetadataName(TargetInterfaceName);
            if (iOptionsSymbol == null)
            {
                // This project doesn't reference IOptions, skip it
                continue;
            }

            // Find all classes implementing IOptions
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = await syntaxTree.GetRootAsync();

                var classDeclarations = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>();

                foreach (var classDecl in classDeclarations)
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
                    if (classSymbol == null || classSymbol.IsAbstract)
                        continue;

                    // Check if implements IOptions (directly or through inheritance)
                    if (ImplementsInterface(classSymbol, iOptionsSymbol))
                    {
                        var sectionName = GetSectionName(classSymbol, compilation);
                        if (sectionName == null)
                        {
                            Console.WriteLine($"  Warning: Class {classSymbol.Name} implements IOptions but has no SectionName property");
                            continue;
                        }

                        Console.WriteLine($"  Found: {classSymbol.Name} (Section: {sectionName})");

                        var properties = GetAllProperties(classSymbol, semanticModel, syntaxTree);
                        optionsClasses.Add(new OptionsClassInfo
                        {
                            ClassName = classSymbol.Name,
                            SectionName = sectionName,
                            Properties = properties
                        });
                    }
                }
            }
        }

        return optionsClasses;
    }

    private static bool ImplementsInterface(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceSymbol)
    {
        return classSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceSymbol));
    }

    private static string? GetSectionName(INamedTypeSymbol classSymbol, Compilation compilation)
    {
        var sectionNameProperty = classSymbol.GetMembers(SectionNameProperty)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => p.IsStatic);

        if (sectionNameProperty == null)
            return null;

        // Try to get the value from syntax using semantic model
        foreach (var syntaxRef in classSymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is not ClassDeclarationSyntax classDecl)
                continue;

            var propDecl = classDecl.Members
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == SectionNameProperty);

            if (propDecl == null)
                continue;

            // Get the semantic model for this syntax tree
            var semanticModel = compilation.GetSemanticModel(syntaxRef.SyntaxTree);

            // Try expression body: public static string SectionName => "value";
            if (propDecl.ExpressionBody?.Expression != null)
            {
                var value = EvaluateExpressionAsString(propDecl.ExpressionBody.Expression, semanticModel);
                if (value != null)
                    return value;
            }

            // Try accessor get expression: public static string SectionName { get => "value"; }
            if (propDecl.AccessorList != null)
            {
                var getter = propDecl.AccessorList.Accessors
                    .FirstOrDefault(a => a.Kind() == SyntaxKind.GetAccessorDeclaration);

                if (getter?.ExpressionBody?.Expression != null)
                {
                    var value = EvaluateExpressionAsString(getter.ExpressionBody.Expression, semanticModel);
                    if (value != null)
                        return value;
                }
            }
        }

        return null;
    }

    private static string? EvaluateExpressionAsString(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        // Handle literal strings: "value"
        if (expression is LiteralExpressionSyntax literal && literal.Token.Value is string strValue)
        {
            return strValue;
        }

        // Handle interpolated strings: $"Prefix:{Constant}"
        if (expression is InterpolatedStringExpressionSyntax interpolated)
        {
            var result = new System.Text.StringBuilder();
            foreach (var content in interpolated.Contents)
            {
                if (content is InterpolatedStringTextSyntax text)
                {
                    result.Append(text.TextToken.ValueText);
                }
                else if (content is InterpolationSyntax interpolation)
                {
                    // Try to evaluate the interpolation expression
                    var constantValue = semanticModel.GetConstantValue(interpolation.Expression);
                    if (constantValue.HasValue && constantValue.Value != null)
                    {
                        result.Append(constantValue.Value.ToString());
                    }
                    else
                    {
                        // Can't evaluate at compile time
                        return null;
                    }
                }
            }
            return result.ToString();
        }

        // Try to get constant value from semantic model (for const fields, etc.)
        var constantValueResult = semanticModel.GetConstantValue(expression);
        if (constantValueResult.HasValue && constantValueResult.Value is string constStr)
        {
            return constStr;
        }

        return null;
    }

    private static List<Models.PropertyInfo> GetAllProperties(
        INamedTypeSymbol classSymbol,
        SemanticModel semanticModel,
        SyntaxTree syntaxTree)
    {
        var properties = new List<Models.PropertyInfo>();
        var processedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        void ProcessType(INamedTypeSymbol typeSymbol, string prefix = "")
        {
            if (!processedTypes.Add(typeSymbol))
                return; // Avoid circular references

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IPropertySymbol property &&
                    property.DeclaredAccessibility == Accessibility.Public &&
                    !property.IsStatic &&
                    !property.IsIndexer)
                {
                    var propertyName = prefix + property.Name;
                    var defaultValue = GetDefaultValue(property, semanticModel, syntaxTree);

                    // Check if property type is a complex type (class)
                    if (property.Type is INamedTypeSymbol namedType &&
                        namedType.TypeKind == TypeKind.Class &&
                        namedType.SpecialType == SpecialType.None &&
                        !IsSimpleType(namedType))
                    {
                        // Recursively process nested class
                        ProcessType(namedType, propertyName + "__");
                    }
                    else
                    {
                        properties.Add(new Models.PropertyInfo
                        {
                            Name = propertyName,
                            DefaultValue = defaultValue,
                            TypeName = property.Type.ToDisplayString()
                        });
                    }
                }
            }
        }

        ProcessType(classSymbol);
        return properties;
    }

    private static bool IsSimpleType(INamedTypeSymbol type)
    {
        return type.SpecialType != SpecialType.None ||
               type.ToDisplayString() == "string" ||
               type.ToDisplayString() == "System.String";
    }

    private static string? GetDefaultValue(IPropertySymbol property, SemanticModel semanticModel, SyntaxTree syntaxTree)
    {
        // Find the property declaration in syntax tree
        foreach (var syntaxRef in property.DeclaringSyntaxReferences)
        {
            if (syntaxRef.SyntaxTree != syntaxTree)
                continue;

            var syntax = syntaxRef.GetSyntax();
            if (syntax is PropertyDeclarationSyntax propDecl && propDecl.Initializer != null)
            {
                var expression = propDecl.Initializer.Value;
                return EvaluateExpression(expression, semanticModel);
            }
        }

        return null;
    }

    private static string? EvaluateExpression(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        // Handle literal values
        if (expression is LiteralExpressionSyntax literal)
        {
            var value = literal.Token.Value;

            // Don't include empty strings or null
            if (value is string str && string.IsNullOrEmpty(str))
                return null;

            if (value == null)
                return null;

            return value.ToString();
        }

        // Handle object creation: new()
        if (expression is ImplicitObjectCreationExpressionSyntax ||
            expression is ObjectCreationExpressionSyntax)
        {
            return null; // Complex objects don't have simple default values
        }

        // Handle member access like string.Empty
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.ToString() == "string.Empty")
                return null;
        }

        // Handle default(T)
        if (expression is DefaultExpressionSyntax)
        {
            return null;
        }

        // Try to get constant value from semantic model
        var constantValue = semanticModel.GetConstantValue(expression);
        if (constantValue.HasValue)
        {
            var value = constantValue.Value;

            if (value is string str && string.IsNullOrEmpty(str))
                return null;

            if (value == null)
                return null;

            return value.ToString();
        }

        return null;
    }
}
