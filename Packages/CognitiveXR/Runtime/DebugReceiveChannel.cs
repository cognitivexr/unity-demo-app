
using System.Collections.Generic;

namespace CognitiveXR.CogStream
{
    public class DebugReceiveChannel : ResultReceiveChannel
    {
        protected override List<EngineResult> ParseResultPacket(ResultPacket resultPacket)
        {
            EngineResult engineResult = new EngineResult()
            {
                frameId = resultPacket.frameId,
                seconds = resultPacket.seconds,
                nanoseconds = resultPacket.nanoseconds,
                result = System.Text.Encoding.UTF8.GetString(resultPacket.data),
            };

            return new List<EngineResult>{ engineResult };
        }
    }
}
