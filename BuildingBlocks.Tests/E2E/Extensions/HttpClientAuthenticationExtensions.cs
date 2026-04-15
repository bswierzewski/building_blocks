using System.Net.Http.Headers;

namespace BuildingBlocks.Tests.E2E.Extensions;

/// <summary>
/// Applies a bearer token to an HTTP client.
/// </summary>
public static class HttpClientAuthenticationExtensions
{
    public static HttpClient As(this HttpClient httpClient, string? accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(accessToken)
            ? null
            : new AuthenticationHeaderValue("Bearer", accessToken);

        return httpClient;
    }
}