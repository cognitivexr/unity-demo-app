using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

    public async void Open()
    {
        // TODO: implement exception handling

        string[] parts = streamSpec.engineAddress.Split(':');

        IPAddress ipAddress = IPAddress.Parse(parts[0]);
        IPEndPoint endPoint = new IPEndPoint(ipAddress, int.Parse(parts[1]));

        client = new TcpClient();
        NetworkStream netstream = client.GetStream();
        {
            await client.ConnectAsync(endPoint.Address, endPoint.Port);

            string json = streamSpecToJson(streamSpec);

            byte[] jsonAsBytes = Encoding.UTF8.GetBytes(json);
            byte[] lengthInByte = BitConverter.GetBytes(jsonAsBytes.Length);

            byte[] data = new byte[4 + jsonAsBytes.Length];
            Buffer.BlockCopy(lengthInByte, 0, data, 0, lengthInByte.Length);
            Buffer.BlockCopy(jsonAsBytes, 0, data, 4, jsonAsBytes.Length);

            await netstream.WriteAsync(data, 0, data.Length);
            await netstream.FlushAsync();

            NetworkStreamFrameWriter frameWriter = new NetworkStreamFrameWriter(networkStream);
            sendChannel.SetWriter(frameWriter);
            packetScanner = new NetworkStreamResultPacketScanner(networkStream); 
            resultReceiveChannel.SetResultPacketScanner(packetScanner);
        }
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
