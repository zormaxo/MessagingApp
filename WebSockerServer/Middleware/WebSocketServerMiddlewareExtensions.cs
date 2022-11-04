namespace WebSocketServer.Middleware
{
    public static class WebSocketServerMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
        { return builder.UseMiddleware<WebSocketServerMiddleware>(); }

        public static IServiceCollection AddWebSocketServerConnectionManager(this IServiceCollection services)
        { return services.AddSingleton<WebSocketServerConnectionManager>(); }
    }
}