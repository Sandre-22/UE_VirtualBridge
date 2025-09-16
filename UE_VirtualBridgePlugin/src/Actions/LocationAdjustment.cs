/*namespace Loupedeck.UE_VirtualBridgePlugin
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;

    public class LocationAdjustment : PluginDynamicAdjustment
    {
        private Int32 _posZ = 0;  // Instantiate the Z value of the object
        public LocationAdjustment()
    : base(displayName: "ZAdjust", description: "Counts rotation ticks", groupName: "Unreal", hasReset: true)
        {
        }
        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            string endpoint;

            // Load config.json at runtime
            try
            {
                var configText = File.ReadAllText("config.json");
                using var doc = JsonDocument.Parse(configText);
                endpoint = doc.RootElement.GetProperty("UnrealEndpoint").GetString();

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

            // Hardcoded actor path and coordinates for now
            var actorPath = "/Game/Maps/Main.Main:PersistentLevel.Cube_2";
            float x = 200, y = 150, z = 50;

            // Fire-and-forget async task
            Task.Run(async () =>
            {
                var success = await UpdateActorLocationAsync(endpoint, actorPath, x, y, z);
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
    }
}
*/