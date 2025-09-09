namespace Loupedeck.UE_VirtualBridgePlugin
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Actions;
    using Loupedeck.UE_VirtualBridgePlugin.Network;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class UE_VirtualBridgePlugin : Plugin
    {
        private RcWs _ws;
        private CancellationTokenSource _cts;

        // Gets a value indicating whether this is an API-only plugin.
        public override Boolean UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is a Universal plugin or an Application plugin.
        public override Boolean HasNoApplication => true;

        // Initializes a new instance of the plugin class.
        public UE_VirtualBridgePlugin()
        {
            // Initialize the plugin log.
            PluginLog.Init(this.Log);

            // Initialize the plugin resources.
            PluginResources.Init(this.Assembly);
        }

        // This method is called when the plugin is loaded.
        public override void Load()
        {
            // Connect <0.0.0.0> to IP Address of Host computer
            // 30020 should be the websocket server port of the Unreal project
            // To find this, go to Project Settings -> "Remote Control" -> Remote Control Websocket Server Port
            /*_ws = new RcWs("ws://<0.0.0.0>:30020");

            _cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                try
                {
                    await _ws.ConnectAsync(_cts.Token);

                    // Optional: log responses so you can see /remote/info, etc.
                    await _ws.ReceiveLoop(async msg =>
                    {
                        this.Log.Info($"UE WS <- {msg}");
                        await Task.CompletedTask;
                    }, _cts.Token);

                }
                catch (Exception ex)
                {
                    this.Log.Info($"WS connect failed (ok if UE not running yet): {ex.Message}");
                }
            });
            */
        }

        // This method is called when the plugin is unloaded.
        public override void Unload()
        {
            /*_cts?.Cancel();
            _ws?.Dispose();*/
        }
    }
}
