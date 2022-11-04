using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseWebSockets();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Use(
    async (context, next) =>
    {
        //WriteRequestParams(context);

        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            app.Logger.LogInformation("WebSocket Connected");

            //var buffer = new byte[1024 * 4];

            //while (webSocket.State == WebSocketState.Open)
            //{
            //    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            //    handleMessage(result);
            //}

            //void handleMessage(WebSocketReceiveResult result)
            //{
            //    if (result.MessageType == WebSocketMessageType.Text)
            //    {
            //        app.Logger.LogInformation("Message Received");
            //    }
            //    else if (result.MessageType == WebSocketMessageType.Close)
            //    {
            //        app.Logger.LogWarning("WebSocket Connected");
            //    }
            //}

            await Receive(
                webSocket,
                async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        app.Logger.LogInformation("Message Received");
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        app.Logger.LogWarning("WebSocket Connected");
                    }
                });
        }
        else
        {
            app.Logger.LogInformation("Hello from 2nd request delegate.");
            await next();
        }
    });


app.Run(
    async context =>
    {
        app.Logger.LogInformation("Hello from 3rd (terminal) Request Delegate");
        await context.Response.WriteAsync("Hello from 3rd (terminal) Request Delegate");
    });

app.Run();

void WriteRequestParams(HttpContext context)
{
    var text = new StringBuilder();

    text.Append($"Request Method: {context.Request.Method}{Environment.NewLine}");
    text.Append($"Request Protocol: {context.Request.Protocol}{Environment.NewLine}");

    if (context.Request.Headers is not null)
    {
        foreach (var h in context.Request.Headers)
        {
            text.Append($"--> {h.Key} : {h.Value}{Environment.NewLine}");
        }
    }

    app.Logger.LogInformation(text.ToString());
}

static async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
{
    var buffer = new byte[1024 * 4];

    while (socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        handleMessage(result, buffer);
    }
}