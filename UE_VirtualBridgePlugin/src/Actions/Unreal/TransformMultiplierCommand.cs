namespace Loupedeck.UE_VirtualBridgePlugin.Actions.Unreal
{
    using System;

    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class TransformMultiplierCommand : PluginDynamicCommand
    {
        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        private float _max = 32f;
        private float _min = 0.25f;
        public TransformMultiplierCommand()
    : base(displayName: "Transform Multiplier", description: "Determines the increment a transform adjustment has on the object, up to 16x.", groupName: "Unreal###Transform")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            this.unreal._transmult *= 2f;
            if (this.unreal._transmult > this._max){
                this.unreal._transmult = this._min;}
            this.ActionImageChanged();
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            $"Transform Multiplier: {Environment.NewLine}{this.unreal._transmult}";
    }
}