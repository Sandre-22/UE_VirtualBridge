namespace Loupedeck.UE_VirtualBridgePlugin.Actions.Unreal.nDisplayCommands
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class FrustumOverscanAdjustment : PluginDynamicAdjustment
    {
        private float currentLength=1.0f;
        private bool _enabled = true;
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;

        public FrustumOverscanAdjustment() : base(
            displayName: "Adjust Overscan",
            description: "Adjusts camera's focal length by multiple of 5",
            groupName: "Unreal###nDisplay",
            hasReset: true) { }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            float adjustment = (float) diff / 10;
            Task.Run(async () =>
            {
                try
                {
                    bool success = await this.unreal.SetOverscanAsync(this.unreal.UnrealEndpoint, this.currentLength + adjustment);
                    if (success)
                    {
                        this.currentLength += adjustment; // only update the current length if the call was successful
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
                    bool success = await this.unreal.SetEnableOverscanAsync(this.unreal.UnrealEndpoint, this._enabled);
                    if (success)
                    {
                        this.Log.Info($"Success in updating camera FOCAL, current length set at {currentLength}");
                        this._enabled = !this._enabled;
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