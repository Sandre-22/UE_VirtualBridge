namespace Loupedeck.UE_VirtualBridgePlugin.Network
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class NetworkClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public NetworkClient(string baseAddress)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        // Generic send for JSON body
        public async Task<string> SendRequestAsync(string relativeUrl, object payload)
        {
            var jsonBody = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Put, relativeUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Unreal API returned {response.StatusCode}: {responseBody}"
                );
            }

            return responseBody;
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}
