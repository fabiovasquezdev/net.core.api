using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Common.WebSocket;

public static class WebSocketAbstractions
    {
        private readonly static JsonSerializerOptions _jsonSerializerOptions =
            new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

    public static Task SendWebSocketMessageAsync<T>(System.Net.WebSockets.WebSocket ws,
                                                        T message,
                                                        CancellationToken cancellationToken)
        {
            try
            {
                return ws.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, _jsonSerializerOptions)),
                                    WebSocketMessageType.Text,
                                    true,
                                    cancellationToken);
            }
            catch { }

            return Task.CompletedTask;
        }

        private const int BUFF_SIZE = 1024;

    private static Task SendPongAsync(System.Net.WebSockets.WebSocket ws,
                                          CancellationToken cancellationToken) =>
            ws.SendAsync(new([0b1]), WebSocketMessageType.Binary,
                        true, cancellationToken);

    private static async IAsyncEnumerable<string> ReceiveMessagesAsync(System.Net.WebSockets.WebSocket ws,
                                [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            do
            {
                using var m = new MemoryStream();
                WebSocketReceiveResult msg;

                do
                {
                    var buff = new byte[BUFF_SIZE];
                    msg = await ws.ReceiveAsync(new(buff), cancellationToken);
                    if (msg.MessageType == WebSocketMessageType.Close || 
                        msg.MessageType == WebSocketMessageType.Binary)
                        break;
                    
                    Array.Resize(ref buff, msg.Count);
                    await m.WriteAsync(buff, cancellationToken);
                } while (!msg.EndOfMessage);

                if (msg.MessageType == WebSocketMessageType.Close)
                    break;

                await SendPongAsync(ws, cancellationToken);
                if (msg.MessageType == WebSocketMessageType.Binary)
                    continue;

                m.Position = 0;
                using var rst = new StreamReader(m);
                yield return rst.ReadToEnd();
                
            } while (ws.State is WebSocketState.Open);
        }

    public static async Task ReceiveWebSocketMessageAsync<T>(System.Net.WebSockets.WebSocket webSocket,
                                                                 Action<T> action,
                                                                 CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var m in ReceiveMessagesAsync(webSocket, cancellationToken))
                    if (!string.IsNullOrEmpty(m))
                        action(JsonSerializer.Deserialize<T>(m, _jsonSerializerOptions)); 
            }
            catch (Exception ex)
            {
                if (webSocket.State is WebSocketState.Open)
                    await SendWebSocketMessageAsync(webSocket, new { ex.Message }, cancellationToken);
            }
        }

    public static async Task LogReceivedWebSocketMessagesAsync(System.Net.WebSockets.WebSocket webSocket,
                                                                   ILogger logger,
                                                                   CancellationToken cancellationToken,
                                                                   [CallerMemberName] string caller = "")
        {
            try
            {
                await foreach (var m in ReceiveMessagesAsync(webSocket, cancellationToken))
                    if (!string.IsNullOrEmpty(m))
                        logger.LogInformation("[RECEIVED WEBSOCKET MESSAGE] - method: {caller} message: {m}", 
                            caller, m); 
            }
            catch (Exception ex)
            {
                if (webSocket.State is WebSocketState.Open)
                    await SendWebSocketMessageAsync(webSocket, new { ex.Message }, cancellationToken);
            }
        }
    }