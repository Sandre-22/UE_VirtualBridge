namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;
    using System.Net;
    using System.Text.Json;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class ToggleLocGridSnapCommand : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        private bool _gridSnapEnabled = true;
        public ToggleLocGridSnapCommand()
    : base(displayName: "Toggle Translate Grid Snap", description: "Turn positional grid snapping on and off.", groupName: "Unreal###Editor") { }

        protected override void RunCommand(String actionParameter)
        {
            this.Log.Info("=== BUTTON PRESSED ===");
            this.Log.Info($"Current grid snap state: {_gridSnapEnabled}");
            this.Log.Info($"Endpoint: {this.unreal.UnrealEndpoint}");

            Task.Run(async () =>
            {
                try
                {
                    this.Log.Info("Calling SetGridSnappingAsync...");
                    bool success = await this.unreal.ToggleLocGridSnappingAsync(
                        this.unreal.UnrealEndpoint,
                        this._gridSnapEnabled
                    );

                    this.Log.Info($"Success: {success}");

                    if (success)
                    {
                        _gridSnapEnabled = !_gridSnapEnabled;
                        this.ActionImageChanged();
                        this.Log.Info($"Grid snapping toggled to: {_gridSnapEnabled}");
                    }
                    else
                    {
                        this.Log.Error("SetGridSnappingAsync returned false");
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
            return $"Grid Snap{Environment.NewLine}{status}";
        }
    }
}