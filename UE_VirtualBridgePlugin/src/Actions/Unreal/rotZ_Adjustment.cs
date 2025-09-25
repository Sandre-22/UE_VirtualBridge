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

    public class rotZ_Adjustment : PluginDynamicAdjustment
    {
        // TODO: make not hard coded
        string actorPath;

        UnrealRemoteService _unreal = new UnrealRemoteService();
        string endpoint;
        public rotZ_Adjustment()
    : base(displayName: "Yaw", description: "Adjusts actor's Z rotation by 1 tick", groupName: "Unreal###Transform###Rotation", hasReset: true)
        {
            _unreal.ConfigService();
            this.ConfigCall();
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            actorPath = _unreal.FetchActor();
            Task.Run(async () =>
            {
                var (data, roll, pitch, yaw) = await _unreal.GetActorRotationAsync(endpoint, actorPath);
                var success = await _unreal.UpdateActorRotationAsync(endpoint, actorPath, roll, pitch, yaw + diff);
                if (success)
                    this.Log.Info("Actor rotation updated");
                else
                    this.Log.Error("Failed to update actor rotation.");
            });
        }

        protected override void RunCommand(String actionParameter)
        {
            actorPath = _unreal.FetchActor();
            Task.Run(async () =>
            {
                var (data, roll, pitch, yaw) = await _unreal.GetActorRotationAsync(endpoint, actorPath);
                var success = await _unreal.UpdateActorRotationAsync(endpoint, actorPath, roll, pitch, 0f);  // TODO, instead of 0f, can save reset to another value
                if (success)
                    this.Log.Info("Actor rotation updated");
                else
                    this.Log.Error("Failed to update actor rotation.");
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
