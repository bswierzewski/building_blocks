using System.Net.Http.Json;

namespace BuildingBlocks.Infrastructure.Http;

public abstract class BaseHttpClient(HttpClient httpClient)
{
    protected async Task<TResponse> GetAsync<TResponse>(string uri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        return await SendRequestAsync<TResponse>(request, cancellationToken);
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(payload)
        };

        return await SendRequestAsync<TResponse>(request, cancellationToken);
    }

    protected async Task<TResponse> PutAsync<TRequest, TResponse>(string uri, TRequest payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = JsonContent.Create(payload)
        };

        return await SendRequestAsync<TResponse>(request, cancellationToken);
    }

    protected async Task<TResponse> SendRequestAsync<TResponse>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw await ParseExceptionAsync(response, cancellationToken);

        var content = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
            ?? throw new Exception("Brak treści odpowiedzi.");
        return content;
    }

    protected abstract Task<Exception> ParseExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken);
}