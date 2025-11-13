using BuildingBlocks.Tests.EndToEnd.Options;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Tests.EndToEnd.Auth;

/// <summary>
/// Collection fixture that provides a shared authentication token for all E2E tests.
/// The token is initialized once per test collection, reducing authentication overhead.
/// </summary>
/// <remarks>
/// Initializes a new instance of the AuthFixture class.
/// </remarks>
/// <param name="authOptions">The authentication options.</param>
public class AuthFixture(IOptions<AuthOptions> authOptions) : IAsyncLifetime
{

    /// <summary>
    /// Gets the authentication token to be used in HTTP requests.
    /// </summary>
    public string AuthToken { get; private set; } = null!;

    /// <summary>
    /// Initializes the fixture by obtaining an authentication token from the configured provider.
    /// </summary>
    public async Task InitializeAsync()
    {
        var tokenProvider = AuthTokenProviderFactory.Create(authOptions);
        AuthToken = await tokenProvider.GetTokenAsync();
    }

    /// <summary>
    /// Performs cleanup when the test collection is finished.
    /// </summary>
    public Task DisposeAsync() => Task.CompletedTask;
}
