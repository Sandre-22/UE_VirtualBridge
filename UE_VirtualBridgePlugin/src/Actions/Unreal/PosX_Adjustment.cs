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
        private String endpoint;

        private UnrealRemoteService GetUnrealService() => UE_VirtualBridgePlugin.UnrealService;

        public PosX_Adjustment() : base(
            displayName: "pX",
            description: "Adjusts actor's X position by 1 tick",
            groupName: "Unreal###Transform###Location",
            hasReset: true) => this.ConfigCall();

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            try
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
                    var (success, x, y, z) = await unreal.GetActorLocationAsync(endpoint, actorPath);
                    if (success)
                    {
                        var updateSuccess = await unreal.UpdateActorLocationAsync(endpoint, actorPath, x + diff, y, z);
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
            catch (Exception ex)
            {
                this.Log.Error(ex, "Error in ApplyAdjustment");
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            try
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
                    var (success, x, y, z) = await unreal.GetActorLocationAsync(endpoint, actorPath);
                    if (success)
                    {
                        var resetSuccess = await unreal.UpdateActorLocationAsync(endpoint, actorPath, 0f, y, z);
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
            catch (Exception ex)
            {
                this.Log.Error(ex, "Error in RunCommand");
            }
        }

        private void ConfigCall()
        {
            var configText = File.ReadAllText("config.json");
            using var doc = JsonDocument.Parse(configText);
            var fetchEndpoint = doc.RootElement.GetProperty("UnrealEndpoint").GetString();
            endpoint = fetchEndpoint?.TrimEnd('/') + "/remote/object/call";

            if (String.IsNullOrWhiteSpace(endpoint))
            {
                this.Log.Error("UnrealEndpoint not set in config.json");
            }
        }
    }
}