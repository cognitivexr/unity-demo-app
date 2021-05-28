
public class DebugReceiveChannel : ResultReceiveChannel
{
    public override void Receive(ResultPacket resultPacket)
    {
        EngineResult engineResult = new EngineResult()
        {
            frameId = resultPacket.frameId,
            seconds = resultPacket.seconds,
            nanoseconds = resultPacket.nanoseconds,
            result = System.Text.Encoding.UTF8.GetString(resultPacket.data),
        };

        engineResultQueue.Enqueue(engineResult);
    }
    
}
