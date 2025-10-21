---WHEN INSTALLING LOUPEDECK ON NEW COMPUTER ---

To see plugin on Loupedeck software, must build the plugin first.

* Navigate to C:\Users\<USER>\AppData\Local\Logi\LogiPluginService
* Create Plugins folder
* In command prompt, navigate to the plugin folder, run "dotnet build" to generate the .LINK file
* Plugin should automatically appear in the Loupedeck app (restart may be required)

In Unreal Engine...
* run WebControl.StartServer

for now, the plugin looks for VBconfig.json in C:\Program Files\Logi\LogiPluginService
* make sure the correct VBconfig.json is in here (must be put in as administrator)
* to see correct syntax for VBconfig.json, check out config.example.json
* to modify VBconfig.json in program files, must modify the same file in the plugin's folder 
  and copy/paste into the program file directory.


--- WHEN INSTALLING VIRTUAL BRIDGE PLUGIN ON UNREAL PROJECT ---

VirtualBridgeConfig.json --> IP should point to mini PC with Loupedeck system

For remote setups:
Find the .ini file at {C:\Program Files\Epic Games\UE_{version}\Engine\Config}
Add this to the HOST computer's baseengine.ini file:

[HTTPServer.Listeners]
DefaultBindAddress={address}

Where {address} is the IP address of the HOST computer (where Unreal Engine will run)
Save and rerun the program.



--- IMPORTANT FOR DEBUGGING ---
UE_VirtualBridge log file > C:\Users\<User>\AppData\Local\Logi\LogiPluginService\Logs\plugin_logs
* txt file where all the loupedeck logs are outputted, including errors and dotnet compile updates.
* Will show up on the mini PC or wherever loupedeck is running from.


--- IMPORTANT FILES ---
Loupedeck config file (VBconfig.json *in program files*) --> should point to the host PC's IP4.
Unreal plugin config file (VirtualBridgeConfig.json) --> should point to the mini PC's IP4.
BaseEngine.ini --> should set a DefaultBindAddress for the host computer's IP4.