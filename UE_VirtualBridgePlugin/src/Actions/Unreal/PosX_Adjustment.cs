namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class PosX_Adjustment : PluginDynamicAdjustment
    {
        // TODO: make not hard coded
        string actorPath = "/Game/Maps/Main.Main:PersistentLevel.Cube_2";

        UnrealRemoteService _unreal = new UnrealRemoteService();
        string endpoint;
        string pX;
        public PosX_Adjustment()
    : base(displayName: "pX", description: "Adjusts actor's X position by 1 tick", groupName: "Unreal###Transform###Location", hasReset: true)
        {
            _unreal.ConfigService();
            this.ConfigCall();
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            Task.Run(async () =>
            {
                var (data, x, y, z) = await _unreal.GetActorLocationAsync(endpoint, actorPath);
                var success = await _unreal.UpdateActorLocationAsync(endpoint, actorPath, x + diff, y, z);
                if (success)
                {
                    this.Log.Info("Actor location updated");
                }
                else
                    this.Log.Error("Failed to update actor location.");
            });
        }

        protected override void RunCommand(String actionParameter)
        {
            // Hardcoded actor path and coordinates for now
            var actorPath = "/Game/Maps/Main.Main:PersistentLevel.Cube_2";
            Task.Run(async () =>
            {
                var (data, x, y, z) = await _unreal.GetActorLocationAsync(endpoint, actorPath);
                var success = await _unreal.UpdateActorLocationAsync(endpoint, actorPath, 0f, y, z);  // TODO, instead of 0f, can save reset to another value
                if (success)
                {
                    this.Log.Info("Actor location updated");
                }
                else
                    this.Log.Error("Failed to update actor location.");
            });
            this.AdjustmentValueChanged(); // Notify the Plugin service that the adjustment value has changed.
        }

        // ASYNC API CALLS
        private void ConfigCall()
        {
            try
            {
                var configText = File.ReadAllText("config.json");
                using var doc = JsonDocument.Parse(configText);
                var fetchEndpoint = doc.RootElement.GetProperty("UnrealEndpoint").GetString();
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
    }
}
