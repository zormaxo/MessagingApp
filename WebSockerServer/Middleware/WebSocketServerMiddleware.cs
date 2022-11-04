using System.Net.WebSockets;
using System.Text;

namespace WebSocketServer.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly ILogger<WebSocketServerMiddleware> _logger;
        private readonly WebSocketServerConnectionManager _manager;
        private readonly RequestDelegate _next;

        public WebSocketServerMiddleware(
            RequestDelegate next,
            WebSocketServerConnectionManager manager,
            ILogger<WebSocketServerMiddleware> logger)
        {
            _logger = logger;
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _logger.LogInformation("WebSocket Connected");

                string connId = _manager.AddSocket(webSocket);
                await SendConnIdAsync(webSocket, connId);

                await Receive(
                    webSocket,
                    async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            _logger.LogInformation("Message Received");
                            _logger.LogInformation($"Message: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            _logger.LogWarning("WebSocket Connected");
                        }
                    });
            }
            else
            {
                _logger.LogInformation("Hello from 2nd request delegate.");
                await _next(context);
            }
        }

        private async Task SendConnIdAsync(WebSocket socket, string connId)
        {
            var buffer = Encoding.UTF8.GetBytes($"ConnId: {connId}");
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}