namespace BuildingBlocks.Tests.E2E;

/// <summary>
/// Canonical users used by end-to-end tests backed by real bearer tokens.
/// </summary>
public static class Users
{
    public static string Anonymous => string.Empty;
    public static string User => GetRequiredAccessToken("Authentication__TestUsers__UserToken");
    public static string Admin => GetRequiredAccessToken("Authentication__TestUsers__AdminToken");

    public static string ActiveEnvironment =>
        Environment.GetEnvironmentVariable("ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? "unknown";

    private static string GetRequiredAccessToken(string variableName)
        => Environment.GetEnvironmentVariable(variableName) switch
        {
            { Length: > 0 } token => token,
            _ => throw new InvalidOperationException(
                $"The '{variableName}' environment variable is required for authenticated end-to-end tests. Active environment: '{ActiveEnvironment}'. Add the appropriate token to envs/.env.e2e or envs/.env.e2e.local.")
        };
}