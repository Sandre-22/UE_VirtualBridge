namespace Loupedeck.UE_VirtualBridgePlugin
{
    using System;

    public class ToggleMuteCommand : PluginDynamicCommand
    {
        private Int32 _sign = 1;
        public ToggleMuteCommand()
    : base(displayName: "Toggle Mute", description: "Toggles audio mute state", groupName: "Audio###Group1")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this.Plugin.ClientApplication.SendKeyboardShortcut(VirtualKeyCode.VolumeMute);
            this._sign *= -1;
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            $"Toggle Mute{Environment.NewLine}{this._sign}";
    }
}