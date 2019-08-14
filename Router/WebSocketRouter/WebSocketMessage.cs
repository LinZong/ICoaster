using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICoaster.Router.WebSocketRouter
{
    public static class WebSocketMessage
    {
        public static async Task<(string, byte[], WebSocketReceiveResult)> GetMessageAsync(WebSocket socket)
        {
            var buffer = new byte[4 * 1024];
            var result = await socket.ReceiveAsync(new System.ArraySegment<byte>(buffer), CancellationToken.None);
            return (Encoding.UTF8.GetString(buffer, 0, result.Count), buffer, result);
        }
    }
}