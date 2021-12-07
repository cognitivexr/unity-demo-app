using System.Threading.Tasks;

namespace CognitiveXR.CogStream
{
   public interface IResultPacketScanner
   {
      Task<ResultPacket> Next();
   }
}