namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class ToggleMultiSelectCommand : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        public ToggleMultiSelectCommand()
    : base(displayName: "Toggle Multi Select", description: "Toggles the ability to move multiple actors at once.", groupName: "Unreal")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this.unreal._multiselect = !this.unreal._multiselect;
            this.ActionImageChanged();
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            $"MultiSelect{Environment.NewLine}{this.unreal._multiselect}";
    }
}