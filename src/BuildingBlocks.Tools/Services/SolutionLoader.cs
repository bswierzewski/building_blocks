using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Xml.Linq;

namespace BuildingBlocks.Tools.Services;

/// <summary>
/// Loads solutions and projects using MSBuild workspace.
/// </summary>
internal class SolutionLoader
{
    private readonly MSBuildWorkspace _workspace;

    public SolutionLoader(MSBuildWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
    /// Loads a solution or project file.
    /// </summary>
    /// <param name="inputFile">The .sln, .slnx, or .csproj file to load.</param>
    /// <returns>The loaded solution.</returns>
    public async Task<Solution> LoadAsync(FileInfo inputFile)
    {
        var extension = inputFile.Extension.ToLowerInvariant();

        if (extension == ".slnx")
        {
            Console.WriteLine("Loading .slnx solution...");
            return await LoadSlnxAsync(inputFile.FullName);
        }
        else if (extension == ".sln")
        {
            Console.WriteLine("Loading solution...");
            return await _workspace.OpenSolutionAsync(inputFile.FullName);
        }
        else // .csproj (already validated)
        {
            Console.WriteLine("Loading project...");
            var project = await _workspace.OpenProjectAsync(inputFile.FullName);
            return project.Solution;
        }
    }

    private async Task<Solution> LoadSlnxAsync(string slnxPath)
    {
        var slnxDirectory = Path.GetDirectoryName(slnxPath)
            ?? throw new InvalidOperationException("Invalid .slnx path");

        // Parse .slnx XML file
        var doc = XDocument.Load(slnxPath);
        var projectPaths = doc.Descendants("Project")
            .Select(p => p.Attribute("Path")?.Value)
            .Where(p => p != null)
            .Select(p => Path.GetFullPath(Path.Combine(slnxDirectory, p!)))
            .ToList();

        if (projectPaths.Count == 0)
        {
            throw new InvalidOperationException($"No projects found in .slnx file: {slnxPath}");
        }

        Console.WriteLine($"Found {projectPaths.Count} project(s) in .slnx");

        // Load first project to get a solution
        var firstProject = await _workspace.OpenProjectAsync(projectPaths[0]);
        var solution = firstProject.Solution;

        // Add remaining projects (skip those already loaded as dependencies)
        foreach (var projectPath in projectPaths.Skip(1))
        {
            // Check if project is already in the solution (loaded as a dependency)
            var normalizedPath = Path.GetFullPath(projectPath);
            var alreadyLoaded = solution.Projects.Any(p =>
                Path.GetFullPath(p.FilePath ?? "") == normalizedPath);

            if (alreadyLoaded)
            {
                Console.WriteLine($"  Skipping {Path.GetFileName(projectPath)} (already loaded as dependency)");
                continue;
            }

            try
            {
                var project = await _workspace.OpenProjectAsync(projectPath);
                solution = project.Solution;
                Console.WriteLine($"  Loaded {Path.GetFileName(projectPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Warning: Could not load project {Path.GetFileName(projectPath)}: {ex.Message}");
            }
        }

        return solution;
    }
}
