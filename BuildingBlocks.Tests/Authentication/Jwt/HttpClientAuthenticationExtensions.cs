using System.Net.Http.Headers;

namespace BuildingBlocks.Tests.Authentication.Jwt;

/// <summary>
/// Applies a bearer token to an HTTP client.
/// </summary>
public static class HttpClientAuthenticationExtensions
{
    public static HttpClient AuthenticateWith(this HttpClient httpClient, string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return httpClient;
    }

    public static HttpClient ClearAuthentication(this HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Authorization = null;
        
        return httpClient;
    }
}