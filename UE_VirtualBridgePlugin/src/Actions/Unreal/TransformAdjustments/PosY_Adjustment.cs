namespace Loupedeck.UE_VirtualBridgePlugin.Actions.Unreal.TransformAdjustments
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

    public class PosY_Adjustment : PluginDynamicAdjustment
    {
        private String endpoint;

        private UnrealRemoteService unreal => UE_VirtualBridgePlugin.UnrealService;
        public PosY_Adjustment()
    : base(displayName: "pY", description: "Adjusts actor's Y position by 1 tick", groupName: "Unreal###Transform###Location", hasReset: true) => this.ConfigCall();

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            try
            {
                this.unreal.GetSelections();

                if (this.unreal._multiselect)
                {
                    this.Log.Info($"Multi-Select mode: {this.unreal._actorcount} actors");
                    for (var i = 0; i < this.unreal._actorcount; i++)
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
                                    var updateSuccess = await unreal.UpdateActorLocationAsync(endpoint, actorPath, x, y + diff * this.unreal._transmult, z);
                                    if (updateSuccess)
                                    {
                                        this.Log.Info($"==MULTI== Actor Y position updated: {y + diff * this.unreal._transmult} for {actorPath}");
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

                    var actorPath = this.unreal.GetCurrentActor(this.unreal._actorindex);
                    var localEndpoint = this.endpoint; // Capture endpoint for closure

                    Task.Run(async () =>
                    {
                        try
                        {
                            var (success, x, y, z) = await this.unreal.GetActorLocationAsync(localEndpoint, actorPath);
                            if (success)
                            {
                                var updateSuccess = await unreal.UpdateActorLocationAsync(endpoint, actorPath, x, y + diff * this.unreal._transmult, z);
                                if (updateSuccess)
                                {
                                    this.Log.Info($"Actor Y position updated: {y + diff * this.unreal._transmult}");
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
                    for (var i = 0; i < this.unreal._actorcount; i++)
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
                                    var updateSuccess = await this.unreal.UpdateActorLocationAsync(endpoint, actorPath, x, 0f, z);  // TODO, instead of 0f, can save reset to another value
                                    if (updateSuccess)
                                    {
                                        this.Log.Info($"==MULTI== Actor Y position updated: 0f for {actorPath}");
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

                    var actorPath = this.unreal.GetCurrentActor(this.unreal._actorindex);
                    var localEndpoint = this.endpoint; // Capture endpoint for closure

                    Task.Run(async () =>
                    {
                        try
                        {
                            var (success, x, y, z) = await this.unreal.GetActorLocationAsync(localEndpoint, actorPath);
                            if (success)
                            {
                                var updateSuccess = await this.unreal.UpdateActorLocationAsync(localEndpoint, actorPath, x, 0f, z);
                                if (updateSuccess)
                                {
                                    this.Log.Info($"Actor Y position updated: 0f");
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
