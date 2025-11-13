namespace BuildingBlocks.Tests.EndToEnd.Auth;

/// <summary>
/// Provides JWT authentication tokens for E2E tests.
/// Implementations can fetch tokens dynamically from auth providers or use static tokens.
/// </summary>
public interface IAuthTokenProvider
{
    /// <summary>
    /// Gets a valid JWT token for testing.
    /// </summary>
    /// <returns>JWT token string</returns>
    Task<string> GetTokenAsync();
}
