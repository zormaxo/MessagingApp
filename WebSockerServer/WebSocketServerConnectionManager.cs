using System.Collections.Concurrent;
using System.Net.WebSockets;
using WebSocketServer.Middleware;

namespace WebSocketServer
{
    public class WebSocketServerConnectionManager
    {
        private readonly ILogger<WebSocketServerMiddleware> _logger;

        public WebSocketServerConnectionManager(ILogger<WebSocketServerMiddleware> logger) { _logger = logger; }

        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public string AddSocket(WebSocket socket)
        {
            string connId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connId, socket);
            _logger.LogInformation($"WebSocketServerConnectionManager -> AddSocket: WebSocket added with ID: {connId}");
            return connId;
        }

        public ConcurrentDictionary<string, WebSocket> GetAllSockets() { return _sockets; }
    }
}