using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CognitiveXR.CogStream
{
    public class MediatorClient
    {
        private ClientWebSocket webSocket;
        private readonly Uri uri;

        public MediatorClient(string url)
        {
            uri = new Uri(url);
        }

        public bool IsOpen() => webSocket != null && webSocket.State == WebSocketState.Open;

        public async Task Open()
        {
            await ConnectToServer(uri);
        }

        public async Task Close()
        {
            if (webSocket.State != WebSocketState.Open) return;
            
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
        }
        
        private async Task ConnectToServer(Uri uri)
        {
            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(uri, CancellationToken.None);
                Debug.Log("connected");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<Engine>> GetEngines()
        {
            await SendMessage(MediatorClientMessage.GetServicesMessage());
            Message service = await Receive();
            return service.content.engines;
        }

        public async Task<string> StartEngine(Engine engine)
        {
            Message selectedEngine = MediatorClientMessage.GetSelectEngineMessage(engine);
            await SendMessage(selectedEngine);
            Message engineAddressMessage = await Receive();
            return engineAddressMessage.content.engineAddress;
        }

        private Task SendMessage(Message message)
        {
            return SendMessage(message.ToJson());
        }

        private async Task SendMessage(string message)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task<Message> Receive()
        {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new byte[1024]);

            if (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = null;
                using (var memoryStream = new MemoryStream())
                {
                    do
                    {
                        result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        memoryStream.Write(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
                        {
                            string answer = reader.ReadToEnd();

                            return MediatorClientMessage.Parse(answer);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty,
                            CancellationToken.None);
                    }
                }
            }

            throw new Exception("No connection to the server");
        }
        
    }
}
