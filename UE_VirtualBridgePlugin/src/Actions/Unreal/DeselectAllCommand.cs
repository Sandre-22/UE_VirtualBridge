namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;
    using System.Net;
    using System.Text.Json;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class DeselectAll : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        private bool _hasSelection = true;
        public DeselectAll()
    : base(displayName: "Deselect All", description: "Turn the scalar snapping on and off", groupName: "Unreal###Editor") { }

        protected override void RunCommand(String actionParameter)
        {
            this.Log.Info("=== BUTTON PRESSED ===");
            this.Log.Info($"Endpoint: {this.unreal.UnrealEndpoint}");

            Task.Run(async () =>
            {
                try
                {
                    this.Log.Info("Calling ToggleScaleSnapAsync...");
                    bool success = await this.unreal.DeselectActorsAsync(
                        this.unreal.UnrealEndpoint,
                        this._hasSelection
                    );

                    this.Log.Info($"Success: {success}");

                    if (success)
                    {
                        this.Log.Error("ToggleScaleSnapAsync returned true");
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
            return $"Deselect Actors";
        }
    }
}