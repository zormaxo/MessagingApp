using System.Net.WebSockets;
using System.Text;

namespace WebSocketServer.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly ILogger<WebSocketServerMiddleware> _logger;
        private readonly RequestDelegate _next;

        public WebSocketServerMiddleware(RequestDelegate next, ILogger<WebSocketServerMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _logger.LogInformation("WebSocket Connected");

                await Receive(
                    webSocket,
                    async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            _logger.LogInformation("Message Received");
                            _logger.LogInformation($"Message:omwe {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
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