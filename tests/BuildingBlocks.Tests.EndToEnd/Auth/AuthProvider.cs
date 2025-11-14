namespace BuildingBlocks.Tests.EndToEnd.Auth;

/// <summary>
/// Enum representing available authentication providers for E2E tests.
/// </summary>
public enum AuthProvider
{
    /// <summary>
    /// No authentication provider - tests will run without authentication.
    /// </summary>
    None = 0,

    /// <summary>
    /// Supabase authentication provider.
    /// </summary>
    Supabase,

    /// <summary>
    /// Clerk authentication provider.
    /// </summary>
    Clerk
}
