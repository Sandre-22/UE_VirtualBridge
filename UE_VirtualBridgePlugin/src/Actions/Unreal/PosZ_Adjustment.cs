namespace Loupedeck.UE_VirtualBridgePlugin.Actions
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;
    using Loupedeck.UE_VirtualBridgePlugin.Services;

    public class PosZ_Adjustment : PluginDynamicAdjustment
    {
        private String endpoint;

        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        public PosZ_Adjustment()
    : base(displayName: "pZ", description: "Adjusts actor's Z position by 1 tick", groupName: "Unreal###Transform###Location", hasReset: true) => this.ConfigCall();

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            try
            {
                this.unreal.GetSelections();

                if (this.unreal._multiselect)
                {
                    this.Log.Info($"Multi-Select mode: {this.unreal._actorcount} actors");
                    for (int i = 0; i < this.unreal._actorcount; i++)
                    {
                        var actorPath = this.unreal._multiactors[i];
                        var localEndpoint = this.endpoint; // Capture endpoint for closure

                        Task.Run(async () =>
                        {
                            try
                            {
                                var (success, x, y, z) = await this.unreal.GetActorLocationAsync(localEndpoint, actorPath);
                                if (success)
                                {
                                    var updateSuccess = await this.unreal.UpdateActorLocationAsync(endpoint, actorPath, x, y, z + diff);
                                    if (updateSuccess)
                                    {
                                        this.Log.Info($"==MULTI== Actor Z position updated: {z + diff} for {actorPath}");
                                    }
                                    else
                                    {
                                        this.Log.Error($"==MULTI== Failed to update actor location for {actorPath}");
                                    }
                                }
                                else
                                {
                                    this.Log.Error($"==MULTI== Failed to get current actor location for {actorPath}");
                                }
                            }
                            catch (Exception taskEx)
                            {
                                this.Log.Error(taskEx, "Error in multi-select task");
                            }
                        });
                    }
                }
                else
                {
                    this.Log.Info($"Single-select mode: {this.unreal._actor}");

                    var actorPath = this.unreal._actor;
                    var localEndpoint = this.endpoint; // Capture endpoint for closure

                    Task.Run(async () =>
                    {
                        try
                        {
                            var (success, x, y, z) = await this.unreal.GetActorLocationAsync(localEndpoint, actorPath);
                            if (success)
                            {
                                var updateSuccess = await this.unreal.UpdateActorLocationAsync(endpoint, actorPath, x, y, z + diff);
                                if (updateSuccess)
                                {
                                    this.Log.Info($"Actor Y position updated: {y + diff}");
                                }
                                else
                                {
                                    this.Log.Error("Failed to update actor location");
                                }
                            }
                            else
                            {
                                this.Log.Error("Failed to get current actor location");
                            }
                        }
                        catch (Exception taskEx)
                        {
                            this.Log.Error(taskEx, "Error in single-select task");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                this.Log.Error(ex, "Error in ApplyAdjustment");
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            try
            {
                this.unreal.GetSelections();

                if (this.unreal._multiselect)
                {
                    for (int i = 0; i < this.unreal._actorcount; i++)
                    {
                        var actorPath = this.unreal._multiactors[i];
                        var localEndpoint = this.endpoint; // Capture endpoint for closure

                        Task.Run(async () =>
                        {
                            try
                            {
                                var (success, x, y, z) = await this.unreal.GetActorLocationAsync(localEndpoint, actorPath);
                                if (success)
                                {
                                    var updateSuccess = await this.unreal.UpdateActorLocationAsync(endpoint, actorPath, x, y, 0f);  // TODO, instead of 0f, can save reset to another value
                                    if (updateSuccess)
                                    {
                                        this.Log.Info($"==MULTI== Actor Z position updated: 0f for {actorPath}");
                                    }
                                    else
                                    {
                                        this.Log.Error($"==MULTI== Failed to update actor location for {actorPath}");
                                    }
                                }
                                else
                                {
                                    this.Log.Error($"==MULTI== Failed to get current actor location for {actorPath}");
                                }
                            }
                            catch (Exception taskEx)
                            {
                                this.Log.Error(taskEx, "Error in multi-select task");
                            }
                        });
                    }
                }
                else
                {
                    this.Log.Info($"Single-select mode: {this.unreal._actor}");

                    var actorPath = this.unreal._actor;
                    var localEndpoint = this.endpoint; // Capture endpoint for closure

                    Task.Run(async () =>
                    {
                        try
                        {
                            var (success, x, y, z) = await this.unreal.GetActorLocationAsync(localEndpoint, actorPath);
                            if (success)
                            {
                                var updateSuccess = await this.unreal.UpdateActorLocationAsync(localEndpoint, actorPath, x, y, 0f);
                                if (updateSuccess)
                                {
                                    this.Log.Info($"Actor Z position updated: 0f");
                                }
                                else
                                {
                                    this.Log.Error("Failed to update actor location");
                                }
                            }
                            else
                            {
                                this.Log.Error("Failed to get current actor location");
                            }
                        }
                        catch (Exception taskEx)
                        {
                            this.Log.Error(taskEx, "Error in single-select task");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                this.Log.Error(ex, "Error in RunCommand");
            }
        }

        // ASYNC API CALLS
        private void ConfigCall()
        {
            try
            {
                var configText = File.ReadAllText("config.json");
                using var doc = JsonDocument.Parse(configText);
                var fetchEndpoint = doc.RootElement.GetProperty("UnrealEndpoint").GetString();
                endpoint = fetchEndpoint.TrimEnd('/') + "/remote/object/call";

                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    this.Log.Error("UnrealEndpoint not set in config.json");
                    return;
                }
            }
            catch (Exception ex)
            {
                this.Log.Error(ex, "Failed to load config.json");
                return;
            }
        }
    }
}
