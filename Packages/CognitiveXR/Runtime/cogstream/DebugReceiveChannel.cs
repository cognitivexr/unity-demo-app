
using System.Collections.Generic;

namespace CognitiveXR.CogStream
{
    /// <summary>
    /// Sample implementation for a receive channel
    /// </summary>
    public class DebugReceiveChannel : ResultReceiveChannel
    {
        /// <summary>
        /// Parse a result channel and returns a list of engine results
        /// </summary>
        /// <param name="resultPacket"></param>
        /// <returns></returns>
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
