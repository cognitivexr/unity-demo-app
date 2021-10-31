using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class EngineClient
{
    private readonly StreamSpec streamSpec;
    private NetworkStream networkStream;
    private NetworkStreamResultPacketScanner packetScanner;
    private IFrameSendChannel sendChannel;
    private ResultReceiveChannel resultReceiveChannel;
    private uint streamId;

    private TcpClient client;

    public EngineClient(StreamSpec streamSpec, IFrameSendChannel sendChannel, ResultReceiveChannel resultReceiveChannel, uint streamId = 0)
    {
        this.streamSpec = streamSpec;
        this.sendChannel = sendChannel;
        this.resultReceiveChannel = resultReceiveChannel;
        this.streamId = streamId;
    }

    public void Open()
    {
        // TODO: implement exception handling

        string[] parts = streamSpec.engineAddress.Split(':');

        IPAddress ipAddress = IPAddress.Parse(parts[0]);
        IPEndPoint endPoint = new IPEndPoint(ipAddress, int.Parse(parts[1]));
        client = new TcpClient();
        {
            try
            {
                client.Connect(endPoint);
                networkStream = client.GetStream();
                {
                    string json = streamSpecToJson(streamSpec);

                    byte[] jsonAsBytes = Encoding.UTF8.GetBytes(json);
                    byte[] lengthInByte = BitConverter.GetBytes(jsonAsBytes.Length);

                    byte[] data = new byte[4 + jsonAsBytes.Length];
                    Buffer.BlockCopy(lengthInByte, 0, data, 0, lengthInByte.Length);
                    Buffer.BlockCopy(jsonAsBytes, 0, data, 4, jsonAsBytes.Length);

                    networkStream.Write(data, 0, data.Length);
                    networkStream.Flush();

                    NetworkStreamFrameWriter frameWriter = new NetworkStreamFrameWriter(networkStream);
                    sendChannel.SetWriter(frameWriter);
                    packetScanner =
                        new NetworkStreamResultPacketScanner(networkStream); // todo: do we need to save it here?
                    packetScanner.onReceivedPacket += resultReceiveChannel.Receive;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public void Shutdown()
    {
        
        client.Close();
    }

    public bool isConnected()
    {
        return (client != null ) && (client.Connected);
    }
    
    private string streamSpecToJson(StreamSpec streamSpec)
    {
        string attributes = streamSpec.attributes.Aggregate(
            "\"attributes\": {", 
            (current, streamSpecAttribute)
                => current + $"\"{streamSpecAttribute.Key}\": [\"{streamSpecAttribute.Value[0]}\"],");
        
        attributes = attributes.TrimEnd(',');
        
        attributes += "}";
        
        return  $"{{" +
                $"\"engineAddress\": \"{streamSpec.engineAddress}\"" +
                ", " + attributes +
                $"}}";
    }

    public T GetSendChannel<T>() where T : IFrameSendChannel
    {
        return (T) sendChannel;
    }

    public T GetReceiveChannel<T>() where T : ResultReceiveChannel
    {
        return (T) resultReceiveChannel;
    }
}
