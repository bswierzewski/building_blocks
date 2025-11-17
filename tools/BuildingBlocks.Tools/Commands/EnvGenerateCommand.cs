using System.CommandLine;
using System.CommandLine.Invocation;
using BuildingBlocks.Tools.Services;

namespace BuildingBlocks.Tools.Commands;

/// <summary>
/// Command for generating .env files from IOptions implementations.
/// Scans assemblies recursively to find all configuration classes that implement IOptions
/// or have EnvSectionAttribute, then generates environment variable definitions.
/// </summary>
public static class EnvGenerateCommand
{
    /// <summary>
    /// Creates the 'env generate' command with all its options.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("generate", "Generate .env file from IOptions implementations");

        var assemblyPathOption = new Option<string[]>(
            aliases: ["--assembly", "-a"],
            description: "Path to the assembly to scan (can be specified multiple times)")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };

        var outputOption = new Option<string>(
            aliases: ["--output", "-o"],
            description: "Output .env file path",
            getDefaultValue: () => ".env.example");

        var recursiveOption = new Option<bool>(
            aliases: ["--recursive", "-r"],
            description: "Recursively scan referenced assemblies",
            getDefaultValue: () => true);

        var includeDescriptionsOption = new Option<bool>(
            aliases: ["--descriptions", "-d"],
            description: "Include descriptions as comments in the output",
            getDefaultValue: () => true);

        var overwriteOption = new Option<bool>(
            aliases: ["--force", "-f"],
            description: "Overwrite existing file without prompting",
            getDefaultValue: () => false);

        command.AddOption(assemblyPathOption);
        command.AddOption(outputOption);
        command.AddOption(recursiveOption);
        command.AddOption(includeDescriptionsOption);
        command.AddOption(overwriteOption);

        command.SetHandler(async (InvocationContext context) =>
        {
            var assemblyPaths = context.ParseResult.GetValueForOption(assemblyPathOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption)!;
            var recursive = context.ParseResult.GetValueForOption(recursiveOption);
            var includeDescriptions = context.ParseResult.GetValueForOption(includeDescriptionsOption);
            var overwrite = context.ParseResult.GetValueForOption(overwriteOption);

            var generator = new EnvFileGenerator();
            await generator.GenerateAsync(
                assemblyPaths,
                output,
                recursive,
                includeDescriptions,
                overwrite,
                context.GetCancellationToken());
        });

        return command;
    }
}
