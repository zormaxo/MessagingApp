using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace SignalRServer.Hubs;

//https://stackoverflow.com/questions/13514259/get-number-of-listeners-clients-connected-to-signalr-hub
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    public ChatHub(ILogger<ChatHub> logger) { _logger = logger; }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"--> Connection Opened: {Context.ConnectionId}");
        Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnID", Context.ConnectionId);
        return base.OnConnectedAsync();
    }


    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"--> Connection Closed: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }


    public async Task SendMessageAsync(string message)
    {
        var routeOb = JsonConvert.DeserializeObject<dynamic>(message);
        _logger.LogInformation($"To: {routeOb!.To.ToString()}{Environment.NewLine}Message Recieved on: {Context.ConnectionId}");

        if (routeOb.To.ToString() == string.Empty)
        {
            _logger.LogInformation("Broadcast");
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
        else
        {
            string toClient = routeOb.To;
            _logger.LogInformation($"Targeted on: {toClient}");

            await Clients.Client(toClient).SendAsync("ReceiveMessage", message);
        }
    }
}