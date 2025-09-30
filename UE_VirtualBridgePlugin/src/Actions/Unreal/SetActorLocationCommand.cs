namespace Loupedeck.UE_VirtualBridgePlugin
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;

    public class SetActorLocation : PluginDynamicCommand
    {
        private static readonly HttpClient client = new HttpClient();
        string endpoint;


        public SetActorLocation()
            : base(displayName: "Set Location",
                   description: "Set location to new XYZ coordinate.",
                   groupName: "Unreal")
        {
            // Constructor left clean—no file I/O here
            // Load config.json at runtime
            try
            {
                var configText = File.ReadAllText("config.json");
                using var doc = JsonDocument.Parse(configText);
                string fetchEndpoint = doc.RootElement.GetProperty("UnrealEndpoint").GetString();
                endpoint = fetchEndpoint.TrimEnd('/') + "/remote/object/call";

                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    this.Log.Error("UnrealEndpoint not set in config.json");
                    return;
                }
            }
            catch (Exception ex)
            {
                this.Log.Error(ex, "Failed to load config.json");
                return;
            }

        }

        // On button press
        protected override void RunCommand(string actionParameter)
        {            
            // Hardcoded actor path and coordinates for now
            var actorPath = "/Game/Maps/Main.Main:PersistentLevel.Cube_2";
            

            // Fire-and-forget async task
            Task.Run(async () =>
            {
                var (data, x, y, z) = await GetActorLocationAsync(endpoint, actorPath);
                var success = await UpdateActorLocationAsync(endpoint, actorPath, x, y, z + 10f);
                if (success)
                    this.Log.Info("Actor location updated");
                else
                    this.Log.Error("Failed to update actor location.");
            });
        }

        private async Task<bool> UpdateActorLocationAsync(string endpoint, string actorPath, float x, float y, float z)
        {
            var payload = new
            {
                objectPath = actorPath,
                functionName = "SetActorLocation",
                parameters = new
                {
                    NewLocation = new { x, y, z },
                    bSweep = false
                },
                generateTransaction = true
            };


            var jsonBody = JsonSerializer.Serialize(payload);
            //var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                this.Log.Info($"Unreal responded: {responseBody}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                this.Log.Error(ex, "HTTP request failed");
                return false;
            }
        }

        private async Task<(bool, float, float, float)> GetActorLocationAsync(string endpoint, string actorPath)
        {
            var payload = new
            {
                objectPath = actorPath,
                functionName = "K2_GetActorLocation",
            };

            var jsonBody = JsonSerializer.Serialize(payload);
            //var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement.GetProperty("ReturnValue");

                float x = root.GetProperty("X").GetSingle();
                float y = root.GetProperty("Y").GetSingle();
                float z = root.GetProperty("Z").GetSingle();
                return (true, x, y, z);
            }
            catch (Exception ex)
            {
                this.Log.Error(ex, "HTTP request failed");
                return (false, 0, 0, 0);
            }

        }

        // Display coordinates on the button
        protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize) =>
            $"Set actor location{Environment.NewLine}0X0Y100Z";
    }
}