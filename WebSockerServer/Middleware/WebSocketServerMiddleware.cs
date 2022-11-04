using Newtonsoft.Json;
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
                //Send ConnID Back
                await SendConnIdAsync(webSocket, connId);

                await Receive(
                    webSocket,
                    async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            _logger.LogInformation(
                                $"Message Received. {Environment.NewLine}Message: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
                            await RouteJSONMessageAsync(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).Key;
                            _logger.LogInformation($"Receive -> Close on: {id}");

                            bool v = _manager.GetAllSockets().TryRemove(id, out WebSocket? socket);
                            _logger.LogInformation($"Managed Connections: {_manager.GetAllSockets().Count.ToString()}");

                            if (socket is not null)
                            {
                                await socket.CloseAsync(
                                    result.CloseStatus!.Value,
                                    result.CloseStatusDescription,
                                    CancellationToken.None);
                            }
                        }
                    });
            }
            else
            {
                _logger.LogInformation("Hello from 2nd request delegate.");
                await _next(context);
            }
        }

        private async Task RouteJSONMessageAsync(string message)
        {
            var routeOb = JsonConvert.DeserializeObject<dynamic>(message);
            _logger.LogInformation($"To: {routeOb!.To}");

            if (Guid.TryParse(routeOb.To.ToString(), out Guid guidOutput))
            {
                _logger.LogInformation("Targeted");
                var socket = _manager.GetAllSockets().FirstOrDefault(s => s.Key == routeOb.To.ToString()).Value;
                if (socket != null)
                {
                    if (socket.State == WebSocketState.Open)
                        await socket.SendAsync(
                            Encoding.UTF8.GetBytes(routeOb.Message.ToString()),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                }
                else
                {
                    _logger.LogError("Invalid Recipient");
                }
            }
            else
            {
                _logger.LogInformation("Broadcast");
                foreach (var socket in _manager.GetAllSockets().Select(x => x.Value))
                {
                    if (socket.State == WebSocketState.Open)
                        await socket.SendAsync(
                            Encoding.UTF8.GetBytes(routeOb.Message.ToString()),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                }
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
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}