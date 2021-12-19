using System;
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
        private readonly string uriString;

        public MediatorClient(string url)
        {
            uriString = url;
        }

        public async Task Open()
        {
            Uri uri = new Uri(uriString);
            await ConnectToServer(uri);
        }

        public async void Close()
        {
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


        public Task SendMessage(Message message)
        {
            return SendMessage(message.ToJson());
        }

        public async Task SendMessage(string message)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task Receive()
        {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new byte[1024]);

            while (webSocket.State == WebSocketState.Open)
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
                            string text = reader.ReadToEnd();

                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty,
                            CancellationToken.None);
                    }
                }

            }
        }
    }
}
