using System.Collections.Concurrent;


public abstract class ResultReceiveChannel
{
    protected readonly ConcurrentQueue<EngineResult> engineResultQueue = new ConcurrentQueue<EngineResult>();

    public abstract void Receive(ResultPacket resultPacket);

    public bool TryDequeue<T>(out T engineResult) where T : EngineResult
    {
        bool success = engineResultQueue.TryDequeue(out EngineResult result);
        engineResult = (T) result;
        return success;
    }
}
