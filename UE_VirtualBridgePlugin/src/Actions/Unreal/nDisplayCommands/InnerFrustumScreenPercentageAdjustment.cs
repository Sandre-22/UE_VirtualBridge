namespace Loupedeck.UE_VirtualBridgePlugin.Actions.Unreal.nDisplayCommands
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class InnerFrustumScreenPercentageAdjustment : PluginDynamicAdjustment
    {
        private float current;
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;

        public InnerFrustumScreenPercentageAdjustment() : base(
            displayName: "Frustum Percentage",
            description: "Adjusts camera's focal length by multiple of 5",
            groupName: "Unreal###nDisplay###Frustum",
            hasReset: true) { }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            Task.Run(async () =>
            {
                try
                {
                    bool success = await this.unreal.SetIFPercentAsync(this.unreal.UnrealEndpoint, this.current + diff);
                    if (success)
                    {
                        this.current += diff; // only update the current length if the call was successful
                        this.Log.Info($"Success in updating camera FOCAL, current length set at {current}");
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
                    bool success = await this.unreal.SetIFPercentAsync(this.unreal.UnrealEndpoint, 1.0f);
                    if (success)
                    {
                        this.current = 1f;
                        this.Log.Info($"Success in updating camera FOCAL, current length set at 1");
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