using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SNotiSSL;

namespace ICoaster.Router.WebSocketRouter
{
    public class WsRouter
    {
        private readonly Dictionary<string, WsControllerInfo> _routerTable;
        private readonly IServiceCollection _service;
        public static SNotiClient SNotiCilentSingleton {get;private set;}
        private readonly ILogger<WsRouter> _logger;
        private readonly SNotiClient _SNotiClient;
        public WsRouter(Dictionary<string, WsControllerInfo> routerTable,
                        IServiceCollection services,
                        ILogger<WsRouter> logger,
                        SNotiClient snotiClient)
        {
            _service = services;
            _routerTable = routerTable;
            _logger = logger;
            SNotiCilentSingleton = _SNotiClient = snotiClient;
            System.Console.WriteLine($"Router中的SNoti :{_SNotiClient.GetHashCode()}");
            _SNotiClient.Connect();
        }

        public Task Route(string RequestPath, HttpContext context, WebSocket socket)
        {
            if (_routerTable.ContainsKey(RequestPath))
            {
                var routeInfo = _routerTable[RequestPath];
                var controller = routeInfo.ControllerType;
                var ctolService = _service.BuildServiceProvider().GetService(controller);
                if (ctolService != null)
                {
                    _logger.LogInformation($"Route ws request {RequestPath} to {routeInfo.HandleRequestMethod.Name}");
                    return (routeInfo.HandleRequestMethod.Invoke(ctolService, new object[] { context, socket }) as Task);
                }
            }
            return DefaultHandler(context, socket);
        }

        public async Task DefaultHandler(HttpContext context, WebSocket socket)
        {
            (string message, _, WebSocketReceiveResult result) = await WebSocketMessage.GetMessageAsync(socket);

            var tips = $"Receive message: {message}, but no handler can handle this request.";
            _logger.LogError(tips);

            var error = Encoding.UTF8.GetBytes(tips);
            await socket.SendAsync(new System.ArraySegment<byte>(error), result.MessageType, result.EndOfMessage, CancellationToken.None);
            await socket.CloseAsync(socket.CloseStatus.HasValue ? socket.CloseStatus.Value : WebSocketCloseStatus.PolicyViolation, socket.CloseStatusDescription, CancellationToken.None);
        }
    }
}