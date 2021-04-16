using System.Net.Sockets;

public interface IFramePacketWriter
{
    void Write(FramePacket framePacket);
}
