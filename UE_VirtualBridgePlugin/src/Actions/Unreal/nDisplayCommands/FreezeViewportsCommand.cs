namespace Loupedeck.UE_VirtualBridgePlugin.Actions.nDisplayCommands
{
    using System;
    using System.Net;
    using System.Text.Json;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class FreezeViewportsCommand : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        private bool _enabled = false;
        public FreezeViewportsCommand()
    : base(displayName: "Freeze Viewports", description: "Turn the scalar snapping on and off", groupName: "Unreal###nDisplay") { }

        protected override void RunCommand(String actionParameter)
        {
            this.Log.Info("=== BUTTON PRESSED ===");
            this.Log.Info($"Endpoint: {this.unreal.UnrealEndpoint}");

            Task.Run(async () =>
            {
                try
                {
                    this.Log.Info("Calling ToggleScaleSnapAsync...");
                    bool success = await this.unreal.SetFreezeViewports(
                        this.unreal.UnrealEndpoint,
                        this._enabled
                    );

                    this.Log.Info($"Success: {success}");

                    if (success)
                    {
                        this.Log.Error("ToggleScaleSnapAsync returned true");
                        this._enabled = !this._enabled;
                        this.ActionImageChanged();
                    }
                    else
                    {
                        this.Log.Error("ToggleScaleSnapAsync returned false");
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex, "Error in deselecting all.");
                }
            });
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            string status = this._enabled ? "ON" : "FRZ";
            return $"Viewports {Environment.NewLine}{status}";
        }
    }
}