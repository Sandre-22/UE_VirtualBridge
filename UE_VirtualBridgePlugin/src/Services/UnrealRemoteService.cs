namespace Loupedeck.UE_VirtualBridgePlugin.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Reflection.Metadata.Ecma335;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class UnrealRemoteService
    {
        private static readonly HttpClient client = new HttpClient();

        String _endpoint;
        public String UnrealEndpoint { get; private set; } = "http://192.168.10.209:30010"; // fallback

        public String _actor;
        public String[] _multiactors;  // make a fixed set of slots?
        public Int32 _actorcount;
        public Boolean _hasselection;

        public Int32 _actorindex = 0;
        public Boolean _multiselect = false;
        public float _transmult = 1f;

        public UnrealRemoteService()
        {
            //this.UnrealEndpoint = Config();
        }

        public String FetchActor()
        {
            // OUTDATED
            var jsonContent = File.ReadAllText(@"C:\Users\LDCtrlRoomSecond\Documents\Unreal Projects\VirtualBridgeConnect\selection.json");  // TODO: Make not hard coded or make easily readable to plugin
            var selection = JsonConvert.DeserializeObject<dynamic>(jsonContent);
            this._actor = selection.primarySelection;
            return this._actor;
        }

        public String GetCurrentActor(Int32 index) => this._multiactors[index];

        public String[] ActorGroup()
        {
            var jsonContent = File.ReadAllText(@"C:\Users\LDCtrlRoomSecond\Documents\Unreal Projects\VirtualBridgeConnect\selection.json");  // TODO: Make not hard coded or make easily readable to plugin
            var selection = JsonConvert.DeserializeObject<dynamic>(jsonContent);
            this._multiactors = selection.selectedActors;
            return this._multiactors;
        }

        public Int32 GetActorCount()
        {
            var jsonContent = File.ReadAllText(@"C:\Users\LDCtrlRoomSecond\Documents\Unreal Projects\VirtualBridgeConnect\selection.json");  // TODO: Make not hard coded or make easily readable to plugin
            var selection = JsonConvert.DeserializeObject<dynamic>(jsonContent);
            this._actorcount = selection.count;
            return this._actorcount;
        }

        public void GetSelections()
        {
            var selection = UE_VirtualBridgePlugin.SelectionListener?.CurrentSelection;

            // set variables
            if (selection != null)
            {
                this._actor = selection.PrimarySelection;
                this._multiactors = selection.SelectedActors;
                this._actorcount = selection.Count;
                this._hasselection = selection.HasSelection;
            }
            else
            {
                this._actor = "";
                this._multiactors = new string[0];
                this._actorcount = 0;
                this._hasselection = false;
            }

            this.NormalizeActorIndex();
            
            return;
        }

        private void NormalizeActorIndex()
        {
            if (this._actorindex >= this._actorcount)
            {
                this._actorindex = 0;
            }
        }

        public async Task<bool> UpdateActorLocationAsync(string endpoint, string actorPath, float x, float y, float z)
        /*
         * Resets XYZ coordinates of a specified actor's location in relative space.
         * 
         * PARAMETERS:
         * <string> endpoint -> the URL that connects to the UE WebServer, decoded from config.json
         * <string> actorPath -> the specified path of the requested actor in UE.
         * <float> x -> the desired float point that requested actor's X should be set to.
         * <float> y -> the desired float point that requested actor's Y should be set to.
         * <float> z -> the desired float point that requested actor's Z should be set to.
         * 
         * RETURNS:
         * <bool> -> whether or not the async call was successful (will throw an error if not).
         */
        {
            var payload = new
            {
                objectPath = actorPath,
                functionName = "SetActorLocation",
                parameters = new
                {
                    NewLocation = new { x, y, z },
                    bSweep = false
                },
                generateTransaction = true
            };


            var jsonBody = System.Text.Json.JsonSerializer.Serialize(payload);
            //var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                //this.Log.Info($"Unreal responded: {responseBody}");
                return response.IsSuccessStatusCode;
            }
            catch //(Exception ex)
            {
                //this.Log.Error(ex, "HTTP request failed");
                return false;
            }
        }

        public async Task<(bool success, float x, float y, float z)> GetActorLocationAsync(String endpoint, String actorPath)
        /*
         * Finds the XYZ coordinates in relative space for an actor's location.
         * 
         * PARAMETERS:
         * <string> endpoint -> the URL that connects to the UE WebServer, decoded from config.json.
         * <string> actorPath -> the specified path of the requested actor in UE.
         * 
         * RETURNS:
         * <bool> success -> whether or not the async call was successful (will throw an error if not).
         * <float> x -> the x coordinate of the requested actor. If unsuccessful, will instead return 0f.
         * <float> y -> the y coordinate of the requested actor. If unsuccessful, will instead return 0f.
         * <float> z -> the z coordinate of the requested actor. If unsuccessful, will instead return 0f.
         */
        {
            _endpoint = endpoint.TrimEnd('/') + "/remote/object/call";
            var payload = new
            {
                objectPath = actorPath,
                functionName = "K2_GetActorLocation",
            };

            var jsonBody = System.Text.Json.JsonSerializer.Serialize(payload);
            //var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Put, _endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement.GetProperty("ReturnValue");

                float X = root.GetProperty("X").GetSingle();
                float Y = root.GetProperty("Y").GetSingle();
                float Z = root.GetProperty("Z").GetSingle();
                return (true, X, Y, Z);
            }
            catch //(Exception ex)
            {
                // TODO: for error handling, figure out a way to bypass the protection level
                //Loupedeck.UE_VirtualBridgePlugin.SetActorLocation.Log.Error(ex, "HTTP request failed");
                return (false, 0, 0, 0);
            }
        }

        public async Task<bool> UpdateActorRotationAsync(string endpoint, string actorPath, float roll, float pitch, float yaw)
        /*
         * Resets XYZ coordinates of a specified actor's location in relative space.
         * 
         * PARAMETERS:
         * <string> endpoint -> the URL that connects to the UE WebServer, decoded from config.json
         * <string> actorPath -> the specified path of the requested actor in UE.
         * <float> x -> the desired float point that requested actor's X should be set to.
         * <float> y -> the desired float point that requested actor's Y should be set to.
         * <float> z -> the desired float point that requested actor's Z should be set to.
         * 
         * RETURNS:
         * <bool> -> whether or not the async call was successful (will throw an error if not).
         */
        {
            var payload = new
            {
                objectPath = actorPath,
                functionName = "SetActorRotation",
                parameters = new
                {
                    NewRotation = new { roll, pitch, yaw },
                    bSweep = false
                },
                generateTransaction = true
            };


            var jsonBody = System.Text.Json.JsonSerializer.Serialize(payload);
            //var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                //this.Log.Info($"Unreal responded: {responseBody}");
                return response.IsSuccessStatusCode;
            }
            catch //(Exception ex)
            {
                //this.Log.Error(ex, "HTTP request failed");
                return false;
            }
        }

        public async Task<(bool success, float roll, float pitch, float yaw)> GetActorRotationAsync(string endpoint, string actorPath)
        /*
         * Finds the XYZ coordinates in relative space for an actor's location.
         * 
         * PARAMETERS:
         * <string> endpoint -> the URL that connects to the UE WebServer, decoded from config.json.
         * <string> actorPath -> the specified path of the requested actor in UE.
         * 
         * RETURNS:
         * <bool> success -> whether or not the async call was successful (will throw an error if not).
         * <float> x -> the x coordinate of the requested actor. If unsuccessful, will instead return 0f.
         * <float> y -> the y coordinate of the requested actor. If unsuccessful, will instead return 0f.
         * <float> z -> the z coordinate of the requested actor. If unsuccessful, will instead return 0f.
         */
        {
            _endpoint = endpoint.TrimEnd('/') + "/remote/object/call";
            var payload = new
            {
                objectPath = actorPath,
                functionName = "K2_GetActorRotation",
            };

            var jsonBody = System.Text.Json.JsonSerializer.Serialize(payload);
            //var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Put, _endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement.GetProperty("ReturnValue");

                float ROLL = root.GetProperty("Roll").GetSingle();
                float PITCH = root.GetProperty("Pitch").GetSingle();
                float YAW = root.GetProperty("Yaw").GetSingle();
                return (true, ROLL, PITCH, YAW);
            }
            catch //(Exception ex)
            {
                // TODO: for error handling, figure out a way to bypass the protection level
                //Loupedeck.UE_VirtualBridgePlugin.SetActorLocation.Log.Error(ex, "HTTP request failed");
                return (false, 0, 0, 0);
            }

        }

        public async Task<bool> DeselectActorsAsync(string endpoint, bool hasSelection)
        {
            return await ExecutePresetFunctionAsync(endpoint, "ConsoleCommandsPreset", "Deselect Actors", new { hasSelection = hasSelection});
        }

        // LOCATION GRID SNAP
        public async Task<bool> ToggleLocGridSnappingAsync(string endpoint, bool currentState)
        {
            return await SetLocGridSnappingAsync(endpoint, !currentState);
        }

        public async Task<bool> SetLocGridSnappingAsync(string endpoint, bool enabled)
        {
            return await ExecutePresetFunctionAsync(
                endpoint,
                "ConsoleCommandsPreset",
                "Set Loc Grid Snapping",
                new { bEnabled = enabled }
            );
        }

        public async Task<bool> SetGridSizeAsync(string endpoint, float size)  //implement later
        {
            return await ExecutePresetFunctionAsync(
                endpoint,
                "ConsoleCommandsPreset",
                "Set Loc Grid Snapping",
                new { Size = size }
            );
        }

        // ROTATION ANGLE SNAP
        public async Task<bool> ToggleRotAngleSnappingAsync(string endpoint, bool currentState)
        {
            return await SetRotAngleSnappingAsync(endpoint, !currentState);
        }

        public async Task<bool> SetRotAngleSnappingAsync(string endpoint, bool enabled)
        {
            return await ExecutePresetFunctionAsync(
                endpoint,
                "ConsoleCommandsPreset",
                "Set Rot Angle Snapping",
                new { bEnabled = enabled }
            );
        }

        // SCALE SNAP
        public async Task<bool> ToggleScaleSnappingAsync(string endpoint, bool currentState)
        {
            return await SetScaleSnappingAsync(endpoint, !currentState);
        }

        public async Task<bool> SetScaleSnappingAsync(string endpoint, bool enabled)
        {
            return await ExecutePresetFunctionAsync(
                endpoint,
                "ConsoleCommandsPreset",
                "Set Scale Snapping",
                new { bEnabled = enabled }
            );
        }

        // FOCAL LENGTH
        public async Task<bool> UpdateFocalLength(string endpoint, float length)
        {
            return await ExecutePresetFunctionAsync(endpoint, 
                "ConsoleCommandsPreset", 
                "Adjust Focal", 
                new { FocalLength = length });
        }

        // GLOBAL EXPOSURE
        public async Task<bool> SetGlobalExposureAsync(string endpoint, float value)
        {
            return await this.ExecutePresetPropertyAsync(endpoint,
                "ConsoleCommandsPreset",
                "Exposure Compensation",
                new { PropertyValue = value });
        }

        // INNER FRUSTUM CALLS
        public async Task<bool> SetEnableFrustumAsync(string endpoint, bool enable)
        {
            return await this.ExecutePresetPropertyAsync(endpoint,
                "ConsoleCommandsPreset",
                "Enable Inner Frustum",
                new { PropertyValue = enable });
        }

        public async Task<bool> SetIFPercentAsync(string endpoint, float value)
        {
            return await this.ExecutePresetPropertyAsync(endpoint,
                "ConsoleCommandsPreset",
                "Inner Frustum Screen Percentage",
                new { PropertyValue = value });
        }

        // NODE VIEWPORTS
        public async Task<bool> SetFreezeViewports(string endpoint, bool enable)
        {
            return await this.ExecutePresetPropertyAsync(endpoint,
                "ConsoleCommandsPreset",
                "Freeze Viewports",
                new { PropertyValue = enable });
        }

        public async Task<bool> SetViewportPercentageAsync(string endpoint, float value)
        {
            return await this.ExecutePresetPropertyAsync(endpoint,
                "ConsoleCommandsPreset",
                "Viewports Screen Percentage",
                new { PropertyValue = value });
        }

        // OVERSCAN
        public async Task<bool> SetEnableOverscanAsync(string endpoint, bool enable)
        {
            return await this.ExecutePresetPropertyAsync(endpoint,
                "ConsoleCommandsPreset",
                "Enable Inner Frustum Overscan",
                new { PropertyValue = enable });
        }

        public async Task<bool> SetOverscanAsync(string endpoint, float value)
        {
            return await this.ExecutePresetPropertyAsync(endpoint,
                "ConsoleCommandsPreset",
                "Overscan Multiplier",
                new { PropertyValue = value });
        }

        // RUNS BLUEPRINT PRESET FUNCTIONS IN THE REMOTE CONTROL PRESET
        public async Task<bool> ExecutePresetFunctionAsync(string endpoint, string presetName, string functionName, object parameters = null)
        /*
         * Calls a function exposed in a Remote Control Preset
         * 
         * PARAMETERS:
         * <string> endpoint -> the URL that connects to the UE WebServer
         * <string> presetName -> name of your Remote Control Preset (e.g., "VirtualBridgePreset")
         * <string> functionName -> name of the exposed function
         * <object> parameters -> parameters as an anonymous object
         */
        {
            _endpoint = endpoint.TrimEnd('/') + $"/remote/preset/{presetName}/function/{functionName}";

            var payload = new { parameters = parameters };

            var jsonBody = System.Text.Json.JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Put, _endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                // Log the response for debugging
                System.Diagnostics.Debug.WriteLine($"Response: {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        // CONNECTS TO EXPOSED PROPERTIES IN THE REMOTE CONTROL PRESET
        public async Task<bool> ExecutePresetPropertyAsync(string endpoint, string presetName, string propertyName, object value)
        {
            _endpoint = endpoint.TrimEnd('/') + $"/remote/preset/{presetName}/property/{propertyName}";

            var jsonBody = System.Text.Json.JsonSerializer.Serialize(value);

            using var request = new HttpRequestMessage(HttpMethod.Put, _endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                // Log the response for debugging
                System.Diagnostics.Debug.WriteLine($"Response: {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }


        public void ConfigService()
        {
            try
            {
                var configText = File.ReadAllText("VBconfig.json");
                using var doc = JsonDocument.Parse(configText);
                var fetchEndpoint = doc.RootElement.GetProperty("UnrealEndpoint").GetString();
                _endpoint = fetchEndpoint.TrimEnd('/') + "/remote/object/call";

                if (String.IsNullOrWhiteSpace(_endpoint))
                {
                    //this.Log.Error("UnrealEndpoint not set in config.json");
                    return;
                }
            }
            catch //(Exception ex)
            {
                //this.Log.Error(ex, "Failed to load config.json");
                return;
            }
        }
        /*
        public String Config()
        {
            try
            {
                var configText = File.ReadAllText("VBconfig.json");
                using var doc = JsonDocument.Parse(configText);
                var fetchEndpoint = doc.RootElement.GetProperty("UnrealEndpoint").GetString();
                return fetchEndpoint.TrimEnd('/') + "/remote/object/call";
            }
            catch //(Exception ex)
            {
                //this.Log.Error(ex, "Failed to load config.json");
                return this.UnrealEndpoint;
            }
        }*/


    }
}
