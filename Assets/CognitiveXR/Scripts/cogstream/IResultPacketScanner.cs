

public delegate void OnReceivedPacket(ResultPacket resultPacket);

public interface IResultPacketScanner
{
   ResultPacket Next();
   
   OnReceivedPacket onReceivedPacket { get; set; }
}
