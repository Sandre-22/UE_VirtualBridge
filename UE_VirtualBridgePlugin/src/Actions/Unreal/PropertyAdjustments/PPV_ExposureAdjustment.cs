namespace Loupedeck.UE_VirtualBridgePlugin.Actions.Unreal.ConsoleCommands
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection.Metadata;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class PPV_ExposureAdjustment : PluginDynamicAdjustment
    {
        private String endpoint;
        private float exposureValue = 1.0f;      // TODO: Write an API GET to retrieve current exposure
        private Boolean enabled = true;
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;

        public PPV_ExposureAdjustment() : base(
            displayName: "Global Exposure",
            description: "Adjusts PPV exposure",
            groupName: "Unreal###Post Process Volume",
            hasReset: true) { }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            float adjustment = diff / 10.0f;
            Task.Run(async () =>
            {
                try
                {
                    bool success = await this.unreal.SetGlobalExposureAsync(this.unreal.UnrealEndpoint, this.exposureValue + adjustment);
                    if (success)
                    {
                        this.exposureValue += adjustment; // only update the current length if the call was successful
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
            Task.Run(async () =>
            {
                try
                {
                    if (this.enabled)
                    {
                        Boolean success = await this.unreal.SetGlobalExposureAsync(this.unreal.UnrealEndpoint, 1.0f);
                        if (success)
                        {
                            this.Log.Info("Success in disabling GLOBAL EXPOSURE");
                            this.enabled = false;
                        }
                    }
                    else
                    {
                        Boolean success = await this.unreal.SetGlobalExposureAsync(this.unreal.UnrealEndpoint, this.exposureValue);
                        if (success)
                        {
                            this.Log.Info("Success in enabling GLOBAL EXPOSURE");
                            this.enabled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex, "Error in toggling GLOBAL EXPOSURE");
                }

            });
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