using BuildingBlocks.Tools.Services;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.CommandLine;

namespace BuildingBlocks.Tools;

public class Program
{
    public static int Main(string[] args)
    {
        // Define command-line arguments
        var inputArgument = new Argument<FileInfo>("input-path")
        {
            Description = "Path to solution (.sln, .slnx) or project (.csproj) file"
        };

        var outputOption = new Option<FileInfo?>("--output")
        {
            Description = "Output path for .env.template file. If not specified, generates in solution/project directory"
        };
        outputOption.Aliases.Add("-o");

        // Create root command
        var rootCommand = new RootCommand("BuildingBlocks.Tools - Generate .env files from IOptions implementations")
        {
            inputArgument,
            outputOption
        };

        // Set handler
        rootCommand.SetAction((parseResult) =>
        {
            var inputPath = parseResult.GetValue(inputArgument);
            var output = parseResult.GetValue(outputOption);

            // Validate input file
            if (inputPath == null || !inputPath.Exists)
            {
                var errorMsg = inputPath == null ? "Missing input path" : $"File not found: {inputPath.FullName}";
                Console.Error.WriteLine($"Error: {errorMsg}");
                return 1;
            }

            var extension = inputPath.Extension.ToLowerInvariant();
            if (extension != ".sln" && extension != ".slnx" && extension != ".csproj")
            {
                Console.Error.WriteLine($"Error: Unsupported file type: {extension}");
                Console.Error.WriteLine("Supported types: .sln, .slnx, .csproj");
                return 1;
            }

            // Execute async operation synchronously
            return ExecuteAsync(inputPath, output).GetAwaiter().GetResult();
        });

        return rootCommand.Parse(args).Invoke();
    }

    private static async Task<int> ExecuteAsync(FileInfo inputFile, FileInfo? outputFile)
    {
        try
        {
            // Determine output path
            var outputPath = outputFile?.FullName ??
                Path.Combine(inputFile.DirectoryName ?? Environment.CurrentDirectory, ".env.template");

            Console.WriteLine($"Analyzing: {inputFile.FullName}");
            Console.WriteLine($"Output: {outputPath}");
            Console.WriteLine();

            // Register MSBuild (only if not already registered)
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }

            // Create workspace and load project/solution
            using var workspace = MSBuildWorkspace.Create();
            workspace.RegisterWorkspaceFailedHandler(e =>
            {
                if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                {
                    Console.WriteLine($"Warning: {e.Diagnostic.Message}");
                }
            });

            // Load solution or project
            var solutionLoader = new SolutionLoader(workspace);
            var solution = await solutionLoader.LoadAsync(inputFile);

            Console.WriteLine($"Loaded {solution.Projects.Count()} project(s)");
            Console.WriteLine();

            // Find all IOptions implementations
            var optionsAnalyzer = new OptionsAnalyzer();
            var optionsClasses = await optionsAnalyzer.AnalyzeAsync(solution);

            Console.WriteLine();
            Console.WriteLine($"Found {optionsClasses.Count} configuration class(es)");
            Console.WriteLine();

            if (optionsClasses.Count == 0)
            {
                Console.WriteLine("No IOptions implementations found. No .env file generated.");
                return 0;
            }

            // Generate .env content
            var envGenerator = new EnvFileGenerator();
            var envContent = envGenerator.Generate(optionsClasses);

            // Write to file
            await File.WriteAllTextAsync(outputPath, envContent, System.Text.Encoding.UTF8);
            Console.WriteLine($"Successfully generated: {outputPath}");
            Console.WriteLine($"Total entries: {optionsClasses.Sum(c => c.Properties.Count)}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}
