using System.Net.Http.Json;
using ErrorOr;

namespace BuildingBlocks.Infrastructure.Http;

public abstract class BaseHttpClient(HttpClient httpClient)
{
    protected abstract Task<List<Error>> ParseErrorAsync(HttpResponseMessage response, CancellationToken ct);

    protected async Task<ErrorOr<TResponse>> GetAsync<TResponse>(string uri, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        return await SendRequestAsync<TResponse>(request, ct);
    }

    protected async Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string uri, TRequest payload, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(payload)
        };

        return await SendRequestAsync<TResponse>(request, ct);
    }

    protected async Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string uri, TRequest payload, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = JsonContent.Create(payload)
        };

        return await SendRequestAsync<TResponse>(request, ct);
    }

    protected async Task<ErrorOr<TResponse>> SendRequestAsync<TResponse>(HttpRequestMessage request, CancellationToken ct)
    {
        using var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
            return await ParseErrorAsync(response, ct);

        var content = await response.Content.ReadFromJsonAsync<TResponse>(ct);

        if (content is null)
            return Error.Unexpected("Http.EmptyResponse", "API returned an empty response body.");

        return content;
    }
}