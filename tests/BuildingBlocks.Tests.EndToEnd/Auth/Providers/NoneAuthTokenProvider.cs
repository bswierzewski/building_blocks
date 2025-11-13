namespace BuildingBlocks.Tests.EndToEnd.Auth.Providers;

/// <summary>
/// No-op authentication provider that returns null.
/// Used when AuthProvider is set to None.
/// </summary>
public class NoneAuthTokenProvider : IAuthTokenProvider
{
    /// <summary>
    /// Returns null as no authentication is configured.
    /// </summary>
    /// <returns>Always returns null.</returns>
    public Task<string?> GetTokenAsync()
    {
        return Task.FromResult<string?>(null);
    }
}
