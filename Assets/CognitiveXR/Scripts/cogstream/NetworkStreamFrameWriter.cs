
using System.Net.Sockets;

public class NetworkStreamFrameWriter : IFramePacketWriter
{
    private readonly NetworkStream networkStream;

    public NetworkStreamFrameWriter(NetworkStream stream)
    {
        networkStream = stream;
    }
    
    public void Write(FramePacket framePacket)
    {
        byte[] buffer = framePacket.ToBytes();
        networkStream.Write(buffer, 0, buffer.Length);
    }
}
