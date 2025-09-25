namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class PosX_Adjustment : PluginDynamicAdjustment
    {
        private UnrealRemoteService _unreal => UE_VirtualBridgePlugin.UnrealService;
        private string endpoint;

        public PosX_Adjustment() : base(
            displayName: "pX",
            description: "Adjusts actor's X position by 1 tick",
            groupName: "Unreal###Transform###Location",
            hasReset: true)
        {
            this.Log.Info("PosX_Adjustment constructor called");
            this.ConfigCall();
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            if (_unreal == null)
            {
                this.Log.Error("UnrealService is not available");
                return;
            }

            var actorPath = _unreal.FetchActor();
            if (string.IsNullOrEmpty(actorPath))
            {
                this.Log.Warning("No actor selected");
                return;
            }

            Task.Run(async () =>
            {
                var (success, x, y, z) = await _unreal.GetActorLocationAsync(endpoint, actorPath);
                if (success)
                {
                    var updateSuccess = await _unreal.UpdateActorLocationAsync(endpoint, actorPath, x + diff, y, z);
                    if (updateSuccess)
                    {
                        this.Log.Info($"Actor X position updated: {x + diff}");
                    }
                    else
                    {
                        this.Log.Error("Failed to update actor location");
                    }
                }
                else
                {
                    this.Log.Error("Failed to get current actor location");
                }
            });
        }

        protected override void RunCommand(String actionParameter)
        {
            if (_unreal == null)
            {
                this.Log.Error("UnrealService is not available");
                return;
            }

            var actorPath = _unreal.FetchActor();
            if (string.IsNullOrEmpty(actorPath))
            {
                this.Log.Warning("No actor selected");
                return;
            }

            Task.Run(async () =>
            {
                var (success, x, y, z) = await _unreal.GetActorLocationAsync(endpoint, actorPath);
                if (success)
                {
                    var resetSuccess = await _unreal.UpdateActorLocationAsync(endpoint, actorPath, 0f, y, z);
                    if (resetSuccess)
                    {
                        this.Log.Info("Actor X position reset to 0");
                        this.AdjustmentValueChanged();
                    }
                    else
                    {
                        this.Log.Error("Failed to reset actor location");
                    }
                }
                else
                {
                    this.Log.Error("Failed to get current actor location");
                }
            });
        }

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
                }
            }
            catch (Exception ex)
            {
                this.Log.Error(ex, "Failed to load config.json");
            }
        }
    }
}