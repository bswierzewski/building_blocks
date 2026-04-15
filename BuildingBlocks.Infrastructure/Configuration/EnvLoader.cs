namespace BuildingBlocks.Infrastructure.Configuration;

/// <summary>
/// Loads layered .env files from the repository envs directory into process environment variables.
/// Later files overwrite values from earlier ones.
/// </summary>
public static class EnvLoader
{
    public static void Load(string? startPath = null)
        => LoadFiles(startPath, ".env", ".env.local");

    public static void LoadIntegration(string? startPath = null)
        => LoadFiles(startPath, ".env", ".env.local", ".env.integration", ".env.integration.local");

    public static void LoadIntegrations(string? startPath = null)
        => LoadIntegration(startPath);

    public static void LoadEndToEnd(string? startPath = null)
        => LoadFiles(startPath, ".env", ".env.local", ".env.e2e", ".env.e2e.local");

    private static void LoadFiles(string? startPath, params string[] fileNames)
    {
        var envDirectory = ResolveEnvDirectory(startPath);

        foreach (var fileName in fileNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var filePath = Path.Combine(envDirectory, fileName);

            if (!File.Exists(filePath))
                continue;

            foreach (var (key, value) in ParseFile(filePath))
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string ResolveEnvDirectory(string? startPath)
    {
        var currentDirectory = ResolveStartDirectory(startPath);

        for (var directory = currentDirectory; directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "envs");

            if (Directory.Exists(candidate))
                return candidate;
        }

        return Path.Combine(currentDirectory.FullName, "envs");
    }

    private static DirectoryInfo ResolveStartDirectory(string? startPath)
    {
        if (string.IsNullOrWhiteSpace(startPath))
            return new DirectoryInfo(Directory.GetCurrentDirectory());

        if (Directory.Exists(startPath))
            return new DirectoryInfo(startPath);

        if (File.Exists(startPath))
            return new FileInfo(startPath).Directory
                ?? new DirectoryInfo(Directory.GetCurrentDirectory());

        return new DirectoryInfo(Path.GetFullPath(startPath));
    }

    private static IEnumerable<KeyValuePair<string, string>> ParseFile(string filePath)
    {
        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (line.StartsWith("export ", StringComparison.Ordinal))
                line = line[7..].Trim();

            var equalsIndex = line.IndexOf('=');

            if (equalsIndex <= 0)
                continue;

            var key = line[..equalsIndex].Trim();
            var value = ParseValue(line[(equalsIndex + 1)..].Trim());

            if (string.IsNullOrWhiteSpace(key))
                continue;

            yield return new KeyValuePair<string, string>(key, value);
        }
    }

    private static string ParseValue(string value)
    {
        if (value.Length >= 2)
        {
            var first = value[0];
            var last = value[^1];

            if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                return value[1..^1];
        }

        var commentIndex = value.IndexOf(" #", StringComparison.Ordinal);

        return commentIndex >= 0
            ? value[..commentIndex].TrimEnd()
            : value;
    }
}