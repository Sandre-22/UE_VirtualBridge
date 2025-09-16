namespace Loupedeck.UE_VirtualBridgePlugin.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck;

    public class UnrealRemoteService
    {
        private static readonly HttpClient client = new HttpClient();
        string _endpoint;

        public UnrealRemoteService()
        {
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


            var jsonBody = JsonSerializer.Serialize(payload);
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
            catch (Exception ex)
            {
                //this.Log.Error(ex, "HTTP request failed");
                return false;
            }
        }

        public async Task<(bool success, float x, float y, float z)> GetActorLocationAsync(string endpoint, string actorPath)
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

            var jsonBody = JsonSerializer.Serialize(payload);
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
            catch (Exception ex)
            {
                // TODO: for error handling, figure out a way to bypass the protection level
                //Loupedeck.UE_VirtualBridgePlugin.SetActorLocation.Log.Error(ex, "HTTP request failed");
                return (false, 0, 0, 0);
            }

        }

    }
}
