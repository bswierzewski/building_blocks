namespace BuildingBlocks.Tests.EndToEnd.Auth;

/// <summary>
/// Provides JWT authentication tokens for E2E tests.
/// Implementations can fetch tokens dynamically from auth providers or use static tokens.
/// Each provider manages its own token caching internally.
/// </summary>
public interface IAuthTokenProvider
{
    /// <summary>
    /// Gets a valid JWT token for testing.
    /// Returns null if no authentication is configured (AuthProvider.None).
    /// </summary>
    /// <returns>JWT token string or null</returns>
    Task<string?> GetTokenAsync();
}
