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

    public class PosZ_Adjustment : PluginDynamicAdjustment
    {
        // TODO: make not hard coded
        string actorPath = "/Game/Maps/Main.Main:PersistentLevel.Cube_2";

        UnrealRemoteService _unreal = new UnrealRemoteService();
        string endpoint;
        public PosZ_Adjustment()
    : base(displayName: "pZ", description: "Adjusts actor's Z position by 1 tick", groupName: "Unreal", hasReset: true)
        {
            this.ConfigCall();
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            Task.Run(async () =>
            {
                var (data, x, y, z) = await _unreal.GetActorLocationAsync(endpoint, actorPath);
                var success = await _unreal.UpdateActorLocationAsync(endpoint, actorPath, x, y, z + diff);
                if (success)
                    this.Log.Info("Actor location updated");
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
                var success = await _unreal.UpdateActorLocationAsync(endpoint, actorPath, x, y, 0f);  // TODO, instead of 0f, can save reset to another value
                if (success)
                    this.Log.Info("Actor location updated");
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
