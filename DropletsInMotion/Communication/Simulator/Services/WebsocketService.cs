using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DropletsInMotion.Communication.Simulator.Models;
using Microsoft.Extensions.Configuration;

namespace DropletsInMotion.Communication.Simulator.Services
{
    public class WebsocketService : IWebsocketService
    {
        private readonly IConfiguration _configuration;


        private HttpListener _httpListener;
        public string? Prefix;
        private List<WebSocket> _connectedClients;
        private readonly TaskCompletionSource<bool> _clientConnectionTask = new TaskCompletionSource<bool>();
        private Dictionary<string, TaskCompletionSource<WebSocketMessage<object>>> _pendingRequests = new();
        private bool _isWebsocketRunning = false;

        public WebsocketService(IConfiguration configuration)
        {
            _configuration = configuration;
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
            Console.WriteLine($"WebSocket started at {Prefix}");

            try
            {
                while (!serverCancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext httpContext = await _httpListener.GetContextAsync();

                    if (httpContext.Request.IsWebSocketRequest)
                    {
                        if (_connectedClients.Count >= 1)
                        {
                            //Console.WriteLine("Rejected new connection, only one client is allowed.");
                            httpContext.Response.StatusCode = 409;
                            httpContext.Response.Close();
                            continue;
                        }

                        HttpListenerWebSocketContext webSocketContext = await httpContext.AcceptWebSocketAsync(subProtocol: null);
                        WebSocket webSocket = webSocketContext.WebSocket;

                        _connectedClients.Add(webSocket);
                        _ = Task.Run(() => HandleConnectionAsync(webSocket, CancellationToken.None));  // Use a new cancellation token for client connections

                        Console.WriteLine($"Client connected from {httpContext.Request.RemoteEndPoint}");

                        _clientConnectionTask.TrySetResult(true);
                    }
                    else
                    {
                        httpContext.Response.StatusCode = 400;
                        httpContext.Response.Close();
                        Console.WriteLine("Received non-WebSocket request, responded with 400 Bad Request.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred in WebSocket server: {ex.Message}");
            }
            finally
            {
                // Only stop the listener if explicitly shutting down
                if (serverCancellationToken.IsCancellationRequested)
                {
                    //Console.WriteLine("Server cancellation was called");
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

                    // Handle the messages
                    HandleReceivedMessages(result, buffer);
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket exception: {ex.Message}");
            }
            finally
            {
                _connectedClients.Remove(webSocket);
                Console.WriteLine("Client disconnected");

                webSocket.Dispose();
            }
        }



        private void HandleReceivedMessages(WebSocketReceiveResult result, byte[] buffer)
        {
            //var buffer = new byte[1024 * 4];

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            //var response = JsonSerializer.Deserialize<WebSocketMessage<T>>(message);
            var baseMessage = JsonSerializer.Deserialize<WebSocketMessage<object>>(message);

            if (baseMessage == null || baseMessage.Type == null || baseMessage.Data == null)
            {
                throw new Exception($"Response is missing data: {message}");
            }

            Console.WriteLine($"Received a message with request id {baseMessage.RequestId}");

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

        public async Task<WebSocketMessage<object>> SendRequestAndWaitForResponseAsync(string requestId, string message, CancellationToken cancellationToken)
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
                        //Console.WriteLine($"Removed {webSocket.State}");
                        //_clientConnectionTask.TrySetResult(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error closing WebSocket connection: {ex.Message}");
                    }
                }
            }

            //_connectedClients.Clear();
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
