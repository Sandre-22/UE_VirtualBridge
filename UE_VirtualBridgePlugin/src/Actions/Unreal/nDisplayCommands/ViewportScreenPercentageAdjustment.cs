namespace Loupedeck.UE_VirtualBridgePlugin.Actions.Unreal.nDisplayCommands
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class ViewportScreenPercentageAdjustment : PluginDynamicAdjustment
    {
        private float current=1.0f;
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;

        public ViewportScreenPercentageAdjustment() : base(
            displayName: "View Screen Percent",
            description: "Adjusts the nDisplay's viewport screen percentage by 1/10 per dial movement.",
            groupName: "Unreal###nDisplay",
            hasReset: true) { }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            float adjustment = (float)diff / 10;
            Task.Run(async () =>
            {
                try
                {
                    bool success = await this.unreal.SetViewportPercentageAsync(this.unreal.UnrealEndpoint, this.current + adjustment);
                    if (success)
                    {
                        this.current += adjustment; // only update the current length if the call was successful
                        this.Log.Info($"Success in updating VIEWPORT SCREEN PERCENTAGE, current length set at {current}");
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex, "Error in adjusting focal length");
                }
            });
            
        }

        protected override void RunCommand(String actionParameter)
        {
            Task.Run(async () =>
            {
                try
                {
                    bool success = await this.unreal.UpdateFocalLength(this.unreal.UnrealEndpoint, 1.0f);
                    if (success)
                    {
                        this.Log.Info($"Success in updating camera FOCAL, current length set at {current}");
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex, "Error in rounding out focal length");
                }
            });
        }
    }
}