using WebSocketServer.Middleware;

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

app.UseWebSocketServer();

app.Run(
    async context =>
    {
        app.Logger.LogInformation("Hello from 3rd (terminal) Request Delegate");
        await context.Response.WriteAsync("Hello from 3rd (terminal) Request Delegate");
    });

app.Run();