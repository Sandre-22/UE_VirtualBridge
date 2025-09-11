namespace Loupedeck.UE_VirtualBridgePlugin
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;

    public class SetActorLocation : PluginDynamicCommand
    {
        private static readonly HttpClient client = new HttpClient();

        public SetActorLocation()
            : base(displayName: "Set Location",
                   description: "Set location to new XYZ coordinate.",
                   groupName: "Unreal")
        {
            // Constructor left clean—no file I/O here
        }

        // On button press
        protected override void RunCommand(string actionParameter)
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
                    NewLocation = new { X = x, Y = y, Z = z },
                    bSweep = false                    
                },
                generateTransaction = true
            };

            var jsonBody = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(endpoint, content);
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

        // Display coordinates on the button
        protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize) =>
            $"Set actor location{Environment.NewLine}200, 150, 50";
    }
}
