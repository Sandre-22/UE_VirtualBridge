namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class CyclePrimaryActorCommand : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        public CyclePrimaryActorCommand()
    : base(displayName: "Cycle Primary Actor", description: "When MultiSelect disabled, chooses a specific actor to manipulate from the ones selected.", groupName: "Unreal")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this.unreal.GetSelections();
            
            if (this.unreal._actorindex >= this.unreal._actorcount - 1)
            {
                this.unreal._actorindex = 0;
            }
            else
            {
                this.unreal._actorindex += 1;
            }
                this.ActionImageChanged();
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            $"Current Actor {Environment.NewLine}{this.unreal._actorindex + 1}";
    }
}