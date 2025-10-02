namespace Loupedeck.UE_VirtualBridgePlugin.Services
{
    using System;
    using System.IO;
    using System.Net;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class SelectionListener : IDisposable
    {
        private HttpListener _listener;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly int _port;
        private readonly Action<string> _logInfo;
        private readonly Action<Exception, string> _logError;

        // Store latest selection in memory
        public SelectionData CurrentSelection {  get; private set; }

        public SelectionListener(int port, Action<string> logInfo, Action<Exception, string> logError)
        {
            this._port = port;
            this._logInfo = logInfo;
            this._logError = logError;
            this.CurrentSelection = new SelectionData();  // initialized with an empty selection data set
        }

        public void Start()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://+:{_port}/");
                _listener.Start();

                _cancellationTokenSource = new CancellationTokenSource();

                // Start background listening
                Task.Run(() => ListenAsync(_cancellationTokenSource.Token));

                _logInfo?.Invoke($"Selection listener started on port {_port}");
            } catch (Exception ex)
            {
                _logError?.Invoke(ex, "Failed to start selection listener");
            }
        }

        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();

                    // Process request in background so we keep listening
                    _ = Task.Run(() => ProcessRequest(context), cancellationToken);
                }
                catch (HttpListenerException)
                {
                    // Listener was stopped
                    break;
                }
                catch (Exception ex)
                {
                    _logError?.Invoke(ex, "Error in listener loop");
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/selection")
                {
                    // Read incoming JSON
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string json = reader.ReadToEnd();

                        // parse/store new selection data
                        CurrentSelection = JsonConvert.DeserializeObject<SelectionData>(json);

                        _logInfo?.Invoke($"Received selection: {CurrentSelection.Count} actors");

                        // Send acknowledgement
                        string responseString = "{\"status\":\"received\"}";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentType = "application/json";
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    response.StatusCode = 404;
                }

                response.Close();
            }
            catch (Exception ex)
            {
                _logError?.Invoke(ex, "Error processing request");
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            _logInfo?.Invoke("Selection listener stopped");
        }    

        public void Dispose()
        {
            Stop();
            _listener = null;
            _cancellationTokenSource?.Dispose();
        }
    }

    // Data model for selection with default values
    public class SelectionData
    {
        [JsonProperty("selectedActors")]
        public string[] SelectedActors { get; set; } = new string[0];

        [JsonProperty("primarySelection")]
        public string PrimarySelection { get; set; } = "";

        [JsonProperty("count")]
        public int Count { get; set; } = 0;

        [JsonProperty("hasSelection")]
        public bool HasSelection { get; set; } = false;
    }
}
