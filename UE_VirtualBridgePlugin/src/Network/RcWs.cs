/*
 * Connects a single, long-lived WebSocket to a remote workstation's Unreal Remote Control API. 
 * Provides simple methods to SEND and RECEIVE JSON messages
 */

namespace Loupedeck.UE_VirtualBridgePlugin.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public sealed class RcWs : IDisposable
    {
        private readonly Uri _uri;
        private ClientWebSocket _ws;

        public RcWs(String wsUrl) => _uri = new Uri(wsUrl);

        public async Task ConnectAsync(CancellationToken ct)
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(this._uri, ct);
        }

        public Task SendHttpAsync(string url, string verb, object body, CancellationToken ct)
        {
            var envelope = new
            {
                MessageName = "http",
                Parameters = new { Url = url, Verb = verb.ToUpperInvariant(), Body = body }
            };
            var json = JsonSerializer.Serialize(envelope);
            var bytes = Encoding.UTF8.GetBytes(json);
            return _ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
        }

        public async Task ReceiveLoop(Func<String, Task> onMessage, CancellationToken ct)
        {
            var buf = new byte[8192];
            while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
            {
                var res = await _ws.ReceiveAsync(new ArraySegment<Byte>(buf), ct);
                if (res.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                    
                var text = Encoding.UTF8.GetString(buf, 0, res.Count);
                if (onMessage != null)
                {
                    await onMessage(text);
                }
                    
            }
        }

        public void Dispose() => _ws?.Dispose();
    }
}
