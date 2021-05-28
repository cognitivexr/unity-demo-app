using System;
using System.Collections.Generic;
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

    // todo: expose to channels
    private uint streamId = 42;

    public EngineClient(StreamSpec streamSpec, IFrameSendChannel sendChannel, ResultReceiveChannel resultReceiveChannel)
    {
        this.streamSpec = streamSpec;
        this.sendChannel = sendChannel;
        this.resultReceiveChannel = resultReceiveChannel;
    }

    public void Open()
    {
        // TODO: implement exception handling

        string[] parts = streamSpec.engineAddress.Split(':');

        IPAddress ipAddress = IPAddress.Parse(parts[0]);
        IPEndPoint endPoint = new IPEndPoint(ipAddress, int.Parse(parts[1]));
        TcpClient client = new TcpClient();
        {
            client.Connect(endPoint);
            networkStream = client.GetStream();
            {
                string json = streamSpecToJson(streamSpec);
                Debug.Log(json);

                byte[] jsonAsBytes = Encoding.UTF8.GetBytes(json);
                byte[] lengthInByte = BitConverter.GetBytes(jsonAsBytes.Length);

                byte[] data = new byte[4 + jsonAsBytes.Length];
                Buffer.BlockCopy(lengthInByte, 0, data, 0, lengthInByte.Length);
                Buffer.BlockCopy(jsonAsBytes, 0, data, 4, jsonAsBytes.Length);

                networkStream.Write(data, 0, data.Length);
                networkStream.Flush();

                NetworkStreamFrameWriter frameWriter = new NetworkStreamFrameWriter(networkStream);
                sendChannel.SetWriter(frameWriter);
                packetScanner = new NetworkStreamResultPacketScanner(networkStream); // todo: do we need to save it here?
                packetScanner.onReceivedPacket += resultReceiveChannel.Receive;
            }
        }
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
