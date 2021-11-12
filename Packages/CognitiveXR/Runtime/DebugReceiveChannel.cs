
using System.Threading.Tasks;

public class DebugReceiveChannel : ResultReceiveChannel
{
    protected override EngineResult Receive(ResultPacket resultPacket)
    {
        EngineResult engineResult = new EngineResult()
        {
            frameId = resultPacket.frameId,
            seconds = resultPacket.seconds,
            nanoseconds = resultPacket.nanoseconds,
            result = System.Text.Encoding.UTF8.GetString(resultPacket.data),
        };

        return engineResult;
    }

}
