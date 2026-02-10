using System.Net.Http.Json;

namespace BuildingBlocks.Infrastructure.Http;

public abstract class BaseHttpClient(HttpClient httpClient)
{
        protected abstract Task ParseErrorAndThrowAsync(HttpResponseMessage response, CancellationToken ct);

        protected async Task<TResponse> GetAsync<TResponse>(string uri, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            return await SendRequestAsync<TResponse>(request, ct);
        }

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest payload, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = JsonContent.Create(payload)
            };

            return await SendRequestAsync<TResponse>(request, ct);
        }

        protected async Task<TResponse> PutAsync<TRequest, TResponse>(string uri, TRequest payload, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Content = JsonContent.Create(payload)
            };

            return await SendRequestAsync<TResponse>(request, ct);
        }

        protected async Task<TResponse> SendRequestAsync<TResponse>(HttpRequestMessage request, CancellationToken ct)
        {
            using var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                await ParseErrorAndThrowAsync(response, ct);
                throw new InvalidOperationException("ParseErrorAndThrowAsync must throw an exception.");
            }

            var content = await response.Content.ReadFromJsonAsync<TResponse>(ct);

            if (content is null)
                throw new InvalidOperationException("API returned an empty response body.");

            return content;
        }
}