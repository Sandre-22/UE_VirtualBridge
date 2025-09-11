namespace Loupedeck.UE_VirtualBridgePlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class RcPayloads
    {
        public static Object SetActorRotation(string objectPath, float pitch, float yaw, float roll) => new
        {
            objectPath,
            functionName = "SetActorRotation",
            parameters = new { NewRotation = new { Pitch = pitch, Yaw = yaw, Roll = roll }, bTeleport = false },
            generateTransaction = true
        };
    }
}
