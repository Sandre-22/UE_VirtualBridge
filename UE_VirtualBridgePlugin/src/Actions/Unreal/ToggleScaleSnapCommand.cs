namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;
    using System.Net;
    using System.Text.Json;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class ToggleScaleSnapCommand : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        private bool _gridSnapEnabled = true;
        public ToggleScaleSnapCommand()
    : base(displayName: "Toggle Scale Snap", description: "Turn the scalar snapping on and off", groupName: "Unreal###Editor") { }

        protected override void RunCommand(String actionParameter)
        {
            this.Log.Info("=== BUTTON PRESSED ===");
            this.Log.Info($"Current grid snap state: {_gridSnapEnabled}");
            this.Log.Info($"Endpoint: {this.unreal.UnrealEndpoint}");

            Task.Run(async () =>
            {
                try
                {
                    this.Log.Info("Calling ToggleScaleSnapAsync...");
                    bool success = await this.unreal.ToggleScaleSnappingAsync(
                        this.unreal.UnrealEndpoint,
                        this._gridSnapEnabled
                    );

                    this.Log.Info($"Success: {success}");

                    if (success)
                    {
                        _gridSnapEnabled = !_gridSnapEnabled;
                        this.ActionImageChanged();
                        this.Log.Info($"Scalar snapping toggled to: {_gridSnapEnabled}");
                    }
                    else
                    {
                        this.Log.Error("ToggleScaleSnapAsync returned false");
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex, "Error in toggling scalar snapping");
                }
            });
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            string status = this._gridSnapEnabled ? "ON" : "OFF";
            return $"Scale Snap{Environment.NewLine}{status}";
        }
    }
}