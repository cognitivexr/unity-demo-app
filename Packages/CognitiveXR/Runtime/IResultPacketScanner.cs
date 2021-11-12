

using System.Threading.Tasks;

public delegate void OnReceivedPacket(ResultPacket resultPacket);

public interface IResultPacketScanner
{
   Task<ResultPacket> Next();
   
   OnReceivedPacket onReceivedPacket { get; set; }
}
