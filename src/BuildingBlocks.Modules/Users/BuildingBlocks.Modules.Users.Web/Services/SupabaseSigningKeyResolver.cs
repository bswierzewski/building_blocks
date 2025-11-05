using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Modules.Users.Web.Services;

/// <summary>
/// Custom document retriever for JWKS endpoints that returns JsonWebKeySet.
/// </summary>
internal sealed class JwksDocumentRetriever(HttpClient httpClient) : IDocumentRetriever
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<string> GetDocumentAsync(string address, CancellationToken cancel)
    {
        return await _httpClient.GetStringAsync(address, cancel);
    }
}

/// <summary>
/// Resolves signing keys from Supabase JWKS endpoint.
/// Uses ConfigurationManager for automatic caching and key rotation.
/// ConfigurationManager is registered in DI and will be automatically disposed on app shutdown.
/// </summary>
public sealed class SupabaseSigningKeyResolver
{
    private readonly IConfigurationManager<JsonWebKeySet> _configurationManager;

    /// <summary>
    /// Initializes a new instance of the SupabaseSigningKeyResolver.
    /// </summary>
    /// <param name="configurationManager">Configuration manager for JWKS retrieval (injected from DI)</param>
    public SupabaseSigningKeyResolver(IConfigurationManager<JsonWebKeySet> configurationManager)
    {
        _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
    }

    /// <summary>
    /// Retrieves signing keys from the JWKS endpoint synchronously.
    /// ConfigurationManager handles caching and automatic refresh.
    /// </summary>
    /// <returns>Collection of security keys for JWT validation</returns>
    public IEnumerable<SecurityKey> GetSigningKeys()
    {
        // Use synchronous version since IssuerSigningKeyResolver doesn't support async
        var jwks = _configurationManager.GetConfigurationAsync(CancellationToken.None)
            .GetAwaiter()
            .GetResult();
        return jwks.Keys;
    }
}

/// <summary>
/// Configuration retriever that parses JWKS JSON into JsonWebKeySet.
/// </summary>
internal sealed class JwksConfigurationRetriever : IConfigurationRetriever<JsonWebKeySet>
{
    public async Task<JsonWebKeySet> GetConfigurationAsync(
        string address,
        IDocumentRetriever retriever,
        CancellationToken cancel)
    {
        var json = await retriever.GetDocumentAsync(address, cancel);
        return new JsonWebKeySet(json);
    }
}
