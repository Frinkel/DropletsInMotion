using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace DropletsInMotion.Communication.Simulator.Services
{
    internal class WebsocketService
    {
        private readonly HttpListener _httpListener;
        private readonly string _prefix;
        private readonly List<WebSocket> _connectedClients;
        private readonly TaskCompletionSource<bool> _clientConnectionTask = new TaskCompletionSource<bool>();


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

            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext httpContext = await _httpListener.GetContextAsync();

                if (httpContext.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await httpContext.AcceptWebSocketAsync(subProtocol: null);
                    WebSocket webSocket = webSocketContext.WebSocket;

                    // Add the new client to the list
                    _connectedClients.Add(webSocket);
                    _clientConnectionTask.TrySetResult(true);

                    Console.WriteLine($"Client connected: {httpContext.Request.RemoteEndPoint}");
                    Console.WriteLine($"Total connected clients: {_connectedClients.Count}");

                    // Handle connection in the background
                    _ = Task.Run(() => HandleConnectionAsync(webSocket, cancellationToken));
                }
                else
                {
                    httpContext.Response.StatusCode = 400;
                    httpContext.Response.Close();
                    Console.WriteLine("Received non-WebSocket request, responded with 400 Bad Request.");
                }
            }

            _httpListener.Stop();
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

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received message: {message}");

                    // Optionally, echo the message back to the client
                    //await SendMessageAsync(webSocket, $"Echo: {message}", cancellationToken);
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


        public int GetNumberOfConnectedClients()
        {
            return _connectedClients.Count;
        }
    }
}
