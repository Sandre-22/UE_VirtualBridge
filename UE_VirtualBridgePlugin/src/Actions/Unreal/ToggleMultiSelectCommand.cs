namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class ToggleMultiSelectCommand : PluginDynamicCommand
    {
        readonly UnrealRemoteService _unreal = UE_VirtualBridgePlugin.UnrealService;
        public ToggleMultiSelectCommand()
    : base(displayName: "Toggle Multi Select", description: "Toggles audio mute state", groupName: "Unreal")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this._unreal._multiselect *= -1;
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            $"Toggle MultiSelect{Environment.NewLine}{this._unreal._multiselect}";
    }
}