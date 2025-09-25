namespace Loupedeck.UE_VirtualBridgePlugin
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Actions;
    using Loupedeck.UE_VirtualBridgePlugin.Network;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class UE_VirtualBridgePlugin : Plugin
    {
        public static UnrealRemoteService UnrealService { get; private set; }

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
            var configService = new ConfigService();
            var client = new NetworkClient(configService.UnrealEndpoint);

            UnrealService = new UnrealRemoteService();
            UnrealService.ConfigService();
            
        }

        // This method is called when the plugin is unloaded.
        public override void Unload()
        {
            
        }
    }
}
