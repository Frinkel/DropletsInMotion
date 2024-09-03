using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DropletsInMotion.Services.Websocket
{
    internal class WebsocketService
    {
        private readonly HttpListener _httpListener;
        private readonly string _prefix;

        public WebsocketService(string prefix)
        {
            _httpListener = new HttpListener();
            _prefix = prefix;
            _httpListener.Prefixes.Add(_prefix);
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

                    Console.WriteLine($"Client connected: {httpContext.Request.RemoteEndPoint}"); // CLIENT CONNECTS

                    await HandleConnectionAsync(webSocket, cancellationToken);
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

                    await SendMessageAsync(webSocket, $"Echo: {message}", cancellationToken);
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket exception: {ex.Message}");
            }
            finally
            {
                webSocket.Dispose();
            }
        }

        private async Task SendMessageAsync(WebSocket webSocket, string message, CancellationToken cancellationToken)
        {
            var encodedMessage = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(encodedMessage);

            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}
