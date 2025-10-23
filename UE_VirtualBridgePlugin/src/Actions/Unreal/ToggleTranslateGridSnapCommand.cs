namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;
    using System.Net;
    using System.Text.Json;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class ToggleTranslateGridSnapCommand : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        private bool _gridSnapEnabled = true;
        public ToggleTranslateGridSnapCommand()
    : base(displayName: "Toggle Translate Grid Snap", description: "When MultiSelect disabled, chooses a specific actor to manipulate from the ones selected.", groupName: "Unreal###Editor") { }

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
                    bool success = await this.unreal.SetGridSnappingAsync(
                        this.unreal.UnrealEndpoint,
                        !this._gridSnapEnabled
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