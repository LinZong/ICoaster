using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ICoaster.Router.WebSocketRouter;
using SNotiSSL;
using Newtonsoft.Json.Linq;

namespace ICoaster.WsControllers
{
    [WebSocketController(Path = "snoti")]
    public class SNotiController
    {
        private readonly ILogger<SNotiController> _logger;
        private readonly SNotiClient _client;
        public SNotiController(ILogger<SNotiController> logger)
        {
            _logger = logger;
            _client = WsRouter.SNotiCilentSingleton;
            System.Console.WriteLine($"Controller中的SNoti :{_client.GetHashCode()}");
        }

        [SubPath(Path = "ws")]
        public async Task SNotiPushHandler(HttpContext context, WebSocket socket)
        {
            var handler = new Action<JObject>(async (msg) =>
            {
                var str = msg.ToString();
                var response = Encoding.UTF8.GetBytes(str);
                await socket.SendAsync(new System.ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
            });
            _client.AddMessageHandler(handler);

            var recv = await WebSocketMessage.GetMessageAsync(socket);
            while (!recv.Item3.CloseStatus.HasValue)
            {
                (string message, _, WebSocketReceiveResult result) = recv;
                _logger.LogInformation($"Receive message: {message}");

                var response = Encoding.UTF8.GetBytes($"{message} -- OK!");
                await socket.SendAsync(new System.ArraySegment<byte>(response), result.MessageType, result.EndOfMessage, CancellationToken.None);

                recv = await WebSocketMessage.GetMessageAsync(socket);
            }

            await socket.CloseAsync(recv.Item3.CloseStatus.Value, recv.Item3.CloseStatusDescription, CancellationToken.None);
            _client.RemoveMessageHandler(handler);
        }
    }
}