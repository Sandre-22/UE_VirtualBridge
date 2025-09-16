namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class SetActorLocation : PluginDynamicCommand
    {
        private readonly UnrealRemoteService _unreal;
        float x, y, z;

        public SetActorLocation(UnrealRemoteService unreal)
            : base(displayName: "Set Location",
                   description: "Set location to new XYZ coordinate.",
                   groupName: "Unreal")
        {
            _unreal = unreal;
        }

        // On button press
        protected override void RunCommand(string actionParameter)
        {
            // Hardcoded actor path and coordinates for now
            var actorPath = "/Game/Maps/Main.Main:PersistentLevel.Cube_2";

            Task.Run(async () =>
            {
                // Here you call your service, not handle HttpClient yourself
                var (success, x, y, z) = await _unreal.GetActorLocationAsync(actorPath);

                if (success)
                {
                    this.Log.Info($"Actor location: X={x}, Y={y}, Z={z}");

                    // Example: move actor down 100 units
                    var moved = await _unreal.UpdateActorLocationAsync(actorPath, x, y, z - 100f);

                    if (moved)
                        this.Log.Info("Actor moved successfully");
                    else
                        this.Log.Error("Failed to move actor");
                }
                else
                {
                    this.Log.Error("Failed to get actor location");
                }
            });
        }

        // Display coordinates on the button
        protected override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize) =>
            $"Set actor location{Environment.NewLine}0X0Y100Z";
    }
}
