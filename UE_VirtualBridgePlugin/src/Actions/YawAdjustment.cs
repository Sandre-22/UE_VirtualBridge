namespace Loupedeck.UE_VirtualBridgePlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Loupedeck.UE_VirtualBridgePlugin.Network;

    public sealed class YawAdjustment : PluginDynamicAdjustment
    {
        private readonly RcWs _rc;
        private float _yaw;
        // keep the last yaw and the original yaw in two separate variables

        // The object path must be changed for each button
        private string _objectPath = "/Game/Maps/Main.Main:PersistentLevel.Cube_2";
        private const float StepDegrees = 1.0f;

        public YawAdjustment(RcWs rc)
            : base("Yaw (UE)", "UE Remote Control", "Transform", false, DeviceType.All)
        { _rc = rc; }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            _yaw = Normalize(_yaw + diff * StepDegrees);
            var body = RcPayloads.SetActorRotation(_objectPath, 0f, _yaw, 0f);
            _ = _rc.SendHttpAsync("/remote/object/call", "PUT", body, CancellationToken.None);
            this.AdjustmentValueChanged();
        }

        private static float Normalize(float v)
        {
            while (v > 180f)
                v -= 360f;
            while (v < -180f)
                v += 360f;
            return v;
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            $"Adjust Yaw{Environment.NewLine}{this._yaw}";
    }
}
