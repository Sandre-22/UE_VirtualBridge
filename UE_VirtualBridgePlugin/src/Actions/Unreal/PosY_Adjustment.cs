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

    public class PosY_Adjustment : PluginDynamicAdjustment
    {
        private String endpoint;

        private UnrealRemoteService GetUnrealService() => UE_VirtualBridgePlugin.UnrealService;
        public PosY_Adjustment()
    : base(displayName: "pY", description: "Adjusts actor's Y position by 1 tick", groupName: "Unreal###Transform###Location", hasReset: true) => this.ConfigCall();

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            var unreal = this.GetUnrealService();
            if (unreal == null)
            {
                this.Log.Error("UnrealService is not available");
                return;
            }

            var actorPath = unreal.FetchActor();
            if (String.IsNullOrEmpty(actorPath))
            {
                this.Log.Warning("No actor selected");
                return;
            }

            Task.Run(async () =>
            {
                var (data, x, y, z) = await unreal.GetActorLocationAsync(endpoint, actorPath);
                var success = await unreal.UpdateActorLocationAsync(endpoint, actorPath, x, y + diff, z);
                if (success)
                    this.Log.Info("Actor location updated");
                else
                    this.Log.Error("Failed to update actor location.");
            });
        }

        protected override void RunCommand(String actionParameter)
        {
            var unreal = this.GetUnrealService();
            if (unreal == null)
            {
                this.Log.Error("UnrealService is not available");
                return;
            }

            var actorPath = unreal.FetchActor();
            if (String.IsNullOrEmpty(actorPath))
            {
                this.Log.Warning("No actor selected");
                return;
            }

            Task.Run(async () =>
            {
                var (data, x, y, z) = await unreal.GetActorLocationAsync(endpoint, actorPath);
                var success = await unreal.UpdateActorLocationAsync(endpoint, actorPath, x, 0f, z);  // TODO, instead of 0f, can save reset to another value
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
