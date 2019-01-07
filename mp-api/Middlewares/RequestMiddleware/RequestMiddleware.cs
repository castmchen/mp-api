

namespace mp_api.Middlewares
{

    #region using

    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion


    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                var cancellationToken = httpContext.RequestAborted;

                var json = await AsyncReceiveWebSocketMessage(webSocket, cancellationToken);
                var messageInfo = JsonConvert.DeserializeObject<dynamic>(json);

                switch (messageInfo.Operation)
                {
                    case "":
                        break;
                    default:
                        break;
                }
            } else
            {
                await this._next.Invoke(httpContext);
            }
        }

        public static async Task<string> AsyncReceiveWebSocketMessage(WebSocket webSocket, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var memoryStream = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    result = await webSocket.ReceiveAsync(buffer, cancellationToken);
                    memoryStream.Write(buffer.ToArray(), buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                memoryStream.Seek(0, SeekOrigin.Begin);

                //switch (result.MessageType)
                //{
                //    case WebSocketMessageType.Binary:
                //        break;
                //    case WebSocketMessageType.Close:
                //        break;
                //    case WebSocketMessageType.Text:
                //        break;
                //    default:
                //        break;
                //}

                using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
