namespace Loupedeck.UE_VirtualBridgePlugin.Actions.Unreal
{
    using System;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class TransformMultiplierCommand : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        public TransformMultiplierCommand()
    : base(displayName: "Transform Multiplier", description: "Toggles the ability to move multiple actors at once.", groupName: "Unreal###Transform")
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