namespace Loupedeck.UE_VirtualBridgePlugin.Actions.Unreal.ConsoleCommands
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class PPV_ExposureAdjustment : PluginDynamicAdjustment
    {
        private String endpoint;
        private float exposureValue = 1.0f;      // TODO: Write an API GET to retrieve current exposure
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;

        public PPV_ExposureAdjustment() : base(
            displayName: "Global Exposure",
            description: "Adjusts PPV exposure",
            groupName: "Unreal###Post Process Volume",
            hasReset: true) { }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            Task.Run(async () =>
            {
                try
                {
                    bool success = await this.unreal.SetGlobalExposureAsync(this.unreal.UnrealEndpoint, this.exposureValue + diff/10);
                    if (success)
                    {
                        this.exposureValue += diff/10; // only update the current length if the call was successful
                        this.Log.Info($"Success in updating GLOBAL EXPOSURE, current length set at {this.exposureValue}");
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex, "Error in adjusting GLOBAL EXPOSURE");
                }
            });
            
        }

        protected override void RunCommand(String actionParameter)
        {
            
        }

        private void ConfigCall()
        {
            var configText = File.ReadAllText("VBconfig.json");
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