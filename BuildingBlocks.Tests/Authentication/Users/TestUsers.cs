namespace BuildingBlocks.Tests.Authentication.Users;

/// <summary>
/// Canonical bearer tokens used by tests that need authenticated requests.
/// </summary>
public static class TestUsers
{
    public static string User => GetRequiredToken("Authentication__TestUsers__UserToken");

    public static string Admin => GetRequiredToken("Authentication__TestUsers__AdminToken");

    public static string ActiveEnvironment =>
        Environment.GetEnvironmentVariable("ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? "unknown";

    private static string GetRequiredToken(string variableName)
        => Environment.GetEnvironmentVariable(variableName) switch
        {
            { Length: > 0 } token => token,
            _ => throw new InvalidOperationException(
                $"The '{variableName}' environment variable is required for authenticated tests. Active environment: '{ActiveEnvironment}'.")
        };
}