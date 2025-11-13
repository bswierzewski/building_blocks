namespace BuildingBlocks.Tests.EndToEnd.Auth;

/// <summary>
/// Enum representing available authentication providers for E2E tests.
/// </summary>
public enum AuthProvider
{
    /// <summary>
    /// Supabase authentication provider.
    /// </summary>
    Supabase,

    /// <summary>
    /// Clerk authentication provider.
    /// </summary>
    Clerk
}
