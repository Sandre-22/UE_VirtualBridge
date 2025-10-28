namespace Loupedeck.UE_VirtualBridgePlugin.Actions.Unreal.ConsoleCommands
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class AdjustFocalAdjustment : PluginDynamicAdjustment
    {
        private float currentLength;
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;

        public AdjustFocalAdjustment() : base(
            displayName: "Adjust Focal Length",
            description: "Adjusts camera's focal length by multiple of 5",
            groupName: "Unreal###Camera",
            hasReset: true) { }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            Task.Run(async () =>
            {
                try
                {
                    bool success = await this.unreal.UpdateFocalLength(this.unreal.UnrealEndpoint, this.currentLength + diff);
                    if (success)
                    {
                        this.currentLength += diff; // only update the current length if the call was successful
                        this.Log.Info($"Success in updating camera FOCAL, current length set at {currentLength}");
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
                    bool success = await this.unreal.UpdateFocalLength(this.unreal.UnrealEndpoint, this.RoundToNearestFive(this.currentLength));
                    if (success)
                    {
                        this.currentLength = this.RoundToNearestFive(this.currentLength);
                        this.Log.Info($"Success in updating camera FOCAL, current length set at {currentLength}");
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex, "Error in rounding out focal length");
                }
            });
        }

        private float RoundToNearestFive(float number) => (float)Math.Round(number / 5.0) * 5;
    }
}