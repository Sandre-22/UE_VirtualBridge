namespace Loupedeck.UE_VirtualBridgePlugin.Services
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Loupedeck.UE_VirtualBridgePlugin.Network;

    public class UnrealRemoteService
    {
        private readonly NetworkClient _client;

        public UnrealRemoteService(NetworkClient client) => _client = client;

        // Get actor location
        public async Task<(bool success, float x, float y, float z)> GetActorLocationAsync(string actorPath)
        {
            var payload = new
            {
                objectPath = actorPath,
                functionName = "K2_GetActorLocation"
            };

            try
            {
                var responseBody = await _client.SendRequestAsync("/remote/object/call", payload);

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement.GetProperty("ReturnValue");

                float x = root.GetProperty("X").GetSingle();
                float y = root.GetProperty("Y").GetSingle();
                float z = root.GetProperty("Z").GetSingle();

                return (true, x, y, z);
            }
            catch
            {
                return (false, 0, 0, 0);
            }
        }

        // Update actor location
        public async Task<bool> UpdateActorLocationAsync(string actorPath, float x, float y, float z)
        {
            var payload = new
            {
                objectPath = actorPath,
                functionName = "SetActorLocation",
                parameters = new
                {
                    NewLocation = new { X = x, Y = y, Z = z },
                    bSweep = false
                },
                generateTransaction = true
            };

            try
            {
                await _client.SendRequestAsync("/remote/object/call", payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
