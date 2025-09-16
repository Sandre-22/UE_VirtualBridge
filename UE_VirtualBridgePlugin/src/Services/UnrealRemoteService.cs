namespace Loupedeck.UE_VirtualBridgePlugin.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class UnrealRemoteService
    {
        private readonly HttpClient _client;

        public UnrealRemoteService(HttpClient client)
        {
            _client = client ?? new HttpClient();
        }

        public async Task<bool> UpdateActorLocationAsync(string endpoint, string actorPath, float x, float y, float z)
        {
            var payload = new
            {
                objectPath = actorPath,
                functionName = "SetActorLocation",
                parameters = new
                {
                    NewLocation = new { X = x, Y = y, Z = z },
                    bSweep = false
                },
                generateTransaction = true
            };

            var jsonBody = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await _client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Unreal responded: {responseBody}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP request failed: {ex}");
                return false;
            }
        }

        public async Task<(bool success, float x, float y, float z)> GetActorLocationAsync(string endpoint, string actorPath)
        {
            var payload = new
            {
                objectPath = actorPath,
                functionName = "K2_GetActorLocation"
            };

            var jsonBody = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await _client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return (false, 0, 0, 0);

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement.GetProperty("ReturnValue");

                float x = root.GetProperty("X").GetSingle();
                float y = root.GetProperty("Y").GetSingle();
                float z = root.GetProperty("Z").GetSingle();

                return (true, x, y, z);
            }
            catch
            {
                return (false, 0, 0, 0);
            }
        }
    }
}
