using System;
using System.Net;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using DropletsInMotion.Communication.Simulator.Models;

namespace DropletsInMotion.Communication.Simulator.Services
{
    internal class WebsocketService
    {
        private readonly HttpListener _httpListener;
        private readonly string _prefix;
        private readonly List<WebSocket> _connectedClients;
        private readonly TaskCompletionSource<bool> _clientConnectionTask = new TaskCompletionSource<bool>();
        private readonly Dictionary<string, TaskCompletionSource<WebSocketMessage<object>>> _pendingRequests = new();

        public WebsocketService(string prefix)
        {
            _httpListener = new HttpListener();
            _prefix = prefix;
            _httpListener.Prefixes.Add(_prefix);
            _connectedClients = new List<WebSocket>();
        }

        public async Task StartServerAsync(CancellationToken cancellationToken)
        {
            _httpListener.Start();
            Console.WriteLine($"WebSocket server started at {_prefix}");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext httpContext = await _httpListener.GetContextAsync();

                    if (httpContext.Request.IsWebSocketRequest)
                    {

                        if (_connectedClients.Count >= 1)
                        {
                            Console.WriteLine("Rejected new connection, only one client is allowed.");
                            httpContext.Response.StatusCode = 409;
                            httpContext.Response.Close();
                            continue;
                        }

                        HttpListenerWebSocketContext webSocketContext = await httpContext.AcceptWebSocketAsync(subProtocol: null);
                        WebSocket webSocket = webSocketContext.WebSocket;

                        Console.WriteLine("Client connected");
                        _connectedClients.Add(webSocket);

                        Console.WriteLine($"Client connected: {httpContext.Request.RemoteEndPoint}");
                        Console.WriteLine($"Total connected clients: {_connectedClients.Count}");

                        _ = Task.Run(() => HandleConnectionAsync(webSocket, cancellationToken));

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
                _httpListener.Stop();
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
                // Remove the client from the list when the connection is closed
                _connectedClients.Remove(webSocket);
                Console.WriteLine($"Client disconnected. Total connected clients: {_connectedClients.Count}");

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
            var buffer = new ArraySegment<byte>(encodedMessage);

            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
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


        public int GetNumberOfConnectedClients()
        {
            return _connectedClients.Count;
        }
    }
}
