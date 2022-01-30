using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CognitiveXR.CogStream
{
    /// <summary>
    /// Engine Client manages the connection to the engine and holds sender and receiver components
    /// </summary>
    public class EngineClient
    {
        private readonly StreamSpec streamSpec;
        private NetworkStream networkStream;
        private NetworkStreamResultPacketScanner packetScanner;
        private readonly IFrameSendChannel sendChannel;
        private readonly ResultReceiveChannel resultReceiveChannel;
        private readonly uint streamId;

        private TcpClient client;

        /// <summary>
        /// Create a Engine Client instance 
        /// </summary>
        /// <param name="streamSpec"></param>
        /// <param name="sendChannel"></param>
        /// <param name="resultReceiveChannel"></param>
        /// <param name="streamId"></param>
        public EngineClient(StreamSpec streamSpec, IFrameSendChannel sendChannel,
            ResultReceiveChannel resultReceiveChannel, uint streamId = 0)
        {
            this.streamSpec = streamSpec;
            this.sendChannel = sendChannel;
            this.resultReceiveChannel = resultReceiveChannel;
            this.streamId = streamId;
            this.sendChannel.SetStreamId(this.streamId);
        }

        /// <summary>
        /// Open a connection to the engine
        /// </summary>
        public async void Open()
        {
            string[] parts = streamSpec.engineAddress.Split(':');

            IPAddress ipAddress = IPAddress.Parse(parts[0]);
            IPEndPoint endPoint = new IPEndPoint(ipAddress, int.Parse(parts[1]));

            try
            {
                client = new TcpClient();
                {
                    await client.ConnectAsync(endPoint.Address, endPoint.Port);

                    string json = streamSpec.ToJson(); 

                    byte[] jsonAsBytes = Encoding.UTF8.GetBytes(json);
                    byte[] lengthInByte = BitConverter.GetBytes(jsonAsBytes.Length);

                    byte[] data = new byte[4 + jsonAsBytes.Length];
                    Buffer.BlockCopy(lengthInByte, 0, data, 0, lengthInByte.Length);
                    Buffer.BlockCopy(jsonAsBytes, 0, data, 4, jsonAsBytes.Length);

                    networkStream = client.GetStream();
                    await networkStream.WriteAsync(data, 0, data.Length);
                    await networkStream.FlushAsync();

                    NetworkStreamFrameWriter frameWriter = new NetworkStreamFrameWriter(networkStream);
                    sendChannel.SetWriter(frameWriter);
                    packetScanner = new NetworkStreamResultPacketScanner(networkStream);
                    resultReceiveChannel.SetResultPacketScanner(packetScanner);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        /// <summary>
        /// Returns whether the client is connected or not
        /// </summary>
        /// <returns></returns>
        public bool isConnected()
        {
            return (client != null) && (client.Connected);
        }

        /// <summary>
        /// Converts StreamSpec to json
        /// </summary>
        /// <param name="streamSpec"></param>
        /// <returns></returns>
        private string streamSpecToJson(StreamSpec streamSpec)
        {
            string attributes = streamSpec.attributes.Aggregate(
                "\"attributes\": {",
                (current, streamSpecAttribute)
                    => current + $"\"{streamSpecAttribute.Key}\": [\"{streamSpecAttribute.Value[0]}\"],");

            attributes = attributes.TrimEnd(',');

            attributes += "}";

            return $"{{" +
                   $"\"engineAddress\": \"{streamSpec.engineAddress}\"" +
                   ", " + attributes +
                   $"}}";
        }

        /// <summary>
        /// Returns the send channel of this engine
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSendChannel<T>() where T : IFrameSendChannel
        {
            return (T) sendChannel;
        }

        /// <summary>
        /// Returns the receive channel of this engine
        /// </summary>
        /// <typeparam name="T">where T is of type ResultReceiveChannel</typeparam>
        /// <returns></returns>
        public T GetReceiveChannel<T>() where T : ResultReceiveChannel
        {
            return (T) resultReceiveChannel;
        }
    }
}