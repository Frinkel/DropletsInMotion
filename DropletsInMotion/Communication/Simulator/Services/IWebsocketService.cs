using System.Net.WebSockets;
using DropletsInMotion.Communication.Simulator.Models;

namespace DropletsInMotion.Communication.Simulator.Services
{
    public interface IWebsocketService
    {
        /// <summary>
        /// Starts the WebSocket server asynchronously.
        /// </summary>
        /// <param name="serverCancellationToken">Cancellation token to stop the server.</param>
        Task StartServerAsync(CancellationToken serverCancellationToken);

        /// <summary>
        /// Waits until a client connection is established.
        /// </summary>
        Task WaitForClientConnectionAsync();

        /// <summary>
        /// Sends a message to all connected clients asynchronously.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SendMessageToAllAsync(string message, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a request to all clients and waits for a response with the specified request ID.
        /// </summary>
        /// <param name="requestId">The ID of the request.</param>
        /// <param name="message">The request message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response message from the client.</returns>
        Task<WebSocketMessage<object>> SendRequestAndWaitForResponseAsync(string requestId, string message, CancellationToken cancellationToken);

        /// <summary>
        /// Closes all active WebSocket connections asynchronously.
        /// </summary>
        Task CloseAllConnectionsAsync();

        /// <summary>
        /// Gets the number of currently connected clients.
        /// </summary>
        /// <returns>The count of connected clients.</returns>
        int GetNumberOfConnectedClients();

        /// <summary>
        /// Checks if the WebSocket server is currently running.
        /// </summary>
        /// <returns>True if the WebSocket server is running, otherwise false.</returns>
        bool IsWebsocketRunning();
    }
}
