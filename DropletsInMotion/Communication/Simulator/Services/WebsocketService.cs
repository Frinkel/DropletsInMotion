using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DropletsInMotion.Communication.Simulator.Models;
using DropletsInMotion.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace DropletsInMotion.Communication.Simulator.Services
{
    public class WebsocketService : IWebsocketService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;


        private HttpListener _httpListener;
        public string? Prefix;
        private List<WebSocket> _connectedClients;
        private TaskCompletionSource<bool> _clientConnectionTask = new TaskCompletionSource<bool>();
        private Dictionary<string, TaskCompletionSource<WebSocketMessage<object>>> _pendingRequests = new();
        private bool _isWebsocketRunning = false;

        public event EventHandler? ClientDisconnected;

        public WebsocketService(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;

            Prefix = _configuration["Development:WebsocketHost"];

            if (Prefix == null)
            {
                throw new Exception("Websocket host cannot be null");
            }

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(Prefix);
            _connectedClients = new List<WebSocket>();
            _isWebsocketRunning = false;
        }

        public async Task StartServerAsync(CancellationToken serverCancellationToken)
        {
            _httpListener.Start();
            _isWebsocketRunning = true;

            _logger.Info($"WebSocket started at {Prefix}");

            try
            {
                while (!serverCancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext httpContext = await _httpListener.GetContextAsync();

                    if (httpContext.Request.IsWebSocketRequest)
                    {
                        if (_connectedClients.Count >= 1)
                        {
                            httpContext.Response.StatusCode = 409;
                            httpContext.Response.Close();
                            continue;
                        }

                        HttpListenerWebSocketContext webSocketContext = await httpContext.AcceptWebSocketAsync(subProtocol: null);
                        WebSocket webSocket = webSocketContext.WebSocket;

                        _connectedClients.Add(webSocket);
                        _ = Task.Run(() => HandleConnectionAsync(webSocket, CancellationToken.None));  // Use a new cancellation token for client connections

                        _logger.Debug($"Client connected from {httpContext.Request.RemoteEndPoint}");
                        _clientConnectionTask.TrySetResult(true);
                    }
                    else
                    {
                        _logger.Warning("Received non-WebSocket request, closing.");

                        httpContext.Response.StatusCode = 400;
                        httpContext.Response.Close();
                        Console.WriteLine("Received non-WebSocket request, responded with 400 Bad Request.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred in WebSocket server: {ex.Message}");
            }
            finally
            {
                if (serverCancellationToken.IsCancellationRequested)
                {
                    _httpListener.Stop();
                    _isWebsocketRunning = false;
                }
            }
        }


        public Task WaitForClientConnectionAsync()
        {
            return _clientConnectionTask.Task;
        }

        private async Task HandleConnectionAsync(WebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocket client", cancellationToken);
                        break;
                    }

                    HandleReceivedMessages(result, buffer);
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket exception: {ex.Message}");
                _clientConnectionTask = new TaskCompletionSource<bool>();
                throw;
            }
            finally
            {

                if (!_httpListener.IsListening)
                {
                    _logger.Warning("HttpListener stopped unexpectedly, restarting...");
                    _httpListener.Start();
                }

                _connectedClients.Remove(webSocket);
                webSocket.Dispose();

                _clientConnectionTask = new TaskCompletionSource<bool>();

                ClientDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }


        private void HandleReceivedMessages(WebSocketReceiveResult result, byte[] buffer)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var baseMessage = JsonSerializer.Deserialize<WebSocketMessage<object>>(message);

            if (baseMessage == null || baseMessage.Type == null || baseMessage.Data == null)
            {
                throw new Exception($"Response is missing data: {message}");
            }

            if (_pendingRequests.TryGetValue(baseMessage.RequestId.ToString(), out var tcs))
            {
                tcs.TrySetResult(baseMessage);
            }
        }

        public async Task SendMessageToAllAsync(string message, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            foreach (var client in _connectedClients)
            {
                if (client.State == WebSocketState.Open)
                {
                    tasks.Add(SendMessageAsync(client, message, cancellationToken));
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task SendMessageAsync(WebSocket webSocket, string message, CancellationToken cancellationToken)
        {
            var encodedMessage = Encoding.UTF8.GetBytes(message);
            int bufferSize = 1024 * 32; // Buffer size (32KB)
            int offset = 0;
            int messageLength = encodedMessage.Length;

            while (offset < messageLength)
            {
                int remainingMessageLength = messageLength - offset;
                int chunkSize = Math.Min(bufferSize, remainingMessageLength);
                bool endOfMessage = (offset + chunkSize) >= messageLength;

                var chunkBuffer = new ArraySegment<byte>(encodedMessage, offset, chunkSize);

                await webSocket.SendAsync(chunkBuffer, WebSocketMessageType.Text, endOfMessage, cancellationToken);

                offset += chunkSize;
            }
        }

        public async Task<WebSocketMessage<object>> SendRequestResponseAsync(string requestId, string message, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<WebSocketMessage<object>>();
            _pendingRequests[requestId] = tcs;

            await SendMessageToAllAsync(message, cancellationToken);

            var response = await tcs.Task;

            _pendingRequests.Remove(requestId);

            return response;
        }

        public async Task CloseAllConnectionsAsync()
        {
            foreach (var webSocket in _connectedClients.ToList())
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client connection closed", CancellationToken.None);
                        webSocket.Dispose();
                        _connectedClients.Remove(webSocket);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error closing WebSocket connection: {ex.Message}");
                    }
                }
            }
        }

        public int GetNumberOfConnectedClients()
        {
            return _connectedClients.Count;
        }

        public bool IsWebsocketRunning()
        {
            return _isWebsocketRunning;
        }
    }
}
