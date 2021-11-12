using System.Collections.Concurrent;
using System.Threading.Tasks;


public abstract class ResultReceiveChannel
{
    protected IResultPacketScanner resultPacketScanner;
    protected readonly ConcurrentQueue<EngineResult> engineResultQueue = new ConcurrentQueue<EngineResult>();

    protected abstract EngineResult Receive(ResultPacket resultPacket);

    public void SetResultPacketScanner(IResultPacketScanner resultPacketScanner)
    {
        this.resultPacketScanner = resultPacketScanner;
    }
    
    public bool TryDequeue<T>(out T engineResult) where T : EngineResult
    {
        bool success = engineResultQueue.TryDequeue(out EngineResult result);
        if (success)
        {
            engineResult = (T) result;
        }
        else
        {
            engineResult = null;
        }
        return success;
    }

    public async Task<T> Next<T>() where T : EngineResult
    {
        if (resultPacketScanner != null)
        {
            ResultPacket resultPacket = await resultPacketScanner.Next();
            EngineResult engineResult = Receive(resultPacket);
            return (T) engineResult;
        }
        
        // TODO: error handling
        return null;
    }
}
