namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;
    using System.Net;
    using System.Text.Json;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class ToggleRotAngleSnapCommand : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        private bool _gridSnapEnabled = true;
        public ToggleRotAngleSnapCommand()
    : base(displayName: "Toggle Rotation Angle Snap", description: "Turn the angular snapping on and off", groupName: "Unreal###Editor") { }

        protected override void RunCommand(String actionParameter)
        {
            this.Log.Info("=== BUTTON PRESSED ===");
            this.Log.Info($"Current grid snap state: {_gridSnapEnabled}");
            this.Log.Info($"Endpoint: {this.unreal.UnrealEndpoint}");

            Task.Run(async () =>
            {
                try
                {
                    this.Log.Info("Calling ToggleRotAngleAsync...");
                    bool success = await this.unreal.ToggleRotAngleSnappingAsync(
                        this.unreal.UnrealEndpoint,
                        this._gridSnapEnabled
                    );

                    this.Log.Info($"Success: {success}");

                    if (success)
                    {
                        _gridSnapEnabled = !_gridSnapEnabled;
                        this.ActionImageChanged();
                        this.Log.Info($"Angle snapping toggled to: {_gridSnapEnabled}");
                    }
                    else
                    {
                        this.Log.Error("ToggleRotAngleAsync returned false");
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex, "Error in toggling grid snapping");
                }
            });
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            string status = this._gridSnapEnabled ? "ON" : "OFF";
            return $"Angle Snap{Environment.NewLine}{status}";
        }
    }
}