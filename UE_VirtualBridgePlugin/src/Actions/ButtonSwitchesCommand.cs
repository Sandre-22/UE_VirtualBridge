using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.UE_VirtualBridgePlugin
{
    class ButtonSwitchesCommand : PluginDynamicCommand
    {
        private readonly Boolean[] _switches = new Boolean[4];  // keeps current state of switches

        public ButtonSwitchesCommand() : base()
        {
            for (var i = 0; i < 4; i++)
            {
                // parameter is the switch index
                var actionParameter = i.ToString();

                // add parameter
                this.AddParameter(actionParameter, $"Switch {i}", "Switches");
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            if (Int32.TryParse(actionParameter, out var i))
            {
                // turn the switch
                this._switches[i] = !this._switches[i];

                // inform service that command display name and/or image has changed
                this.ActionImageChanged(actionParameter);
            }
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            if (Int32.TryParse(actionParameter, out var i))
            {
                return $"Switch {i}: {this._switches[i]}";
            }
            else
            {
                return null;
            }
        }
    }
}
