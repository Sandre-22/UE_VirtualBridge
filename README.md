---WHEN INSTALLING ON NEW COMPUTER ---



To see plugin on Loupedeck software, must build the plugin first.

* Navigate to C:\Users\<USER>\AppData\Local\Logi\LogiPluginService
* Create Plugins folder
* In command prompt, navigate to the plugin folder, run "dotnet build" to generate the .LINK file
* Plugin should automatically appear in the Loupedeck app (restart may be required)

In Unreal Engine...
* run WebControl.StartServer

for now, the plugin looks for config.json in C:\Program Files\Logi\LogiPluginService
* make sure the correct config.json is in here (must be put in as administrator)
* to see correct syntax for config.json, check out config.example.json
