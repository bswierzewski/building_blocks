using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BuildingBlocks.Tests.Extensions.Http;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(endpoint, content);
    }

    public static async Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PutAsync(endpoint, content);
    }

    public static async Task<T?> ReadAsJsonAsync<T>(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        // Return default if content is empty or whitespace
        if (string.IsNullOrWhiteSpace(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public static HttpClient WithBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static HttpClient WithoutAuthorization(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }
}
