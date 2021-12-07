using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveXR.CogStream
{
    public abstract class ResultReceiveChannel
    {
        private IResultPacketScanner resultPacketScanner;

        protected abstract List<EngineResult> ParseResultPacket(ResultPacket resultPacket);

        public void SetResultPacketScanner(IResultPacketScanner inResultPacketScanner)
        {
            this.resultPacketScanner = inResultPacketScanner;
        }

        public async Task<List<T>> Next<T>() where T : EngineResult
        {
            if (resultPacketScanner != null)
            {
                ResultPacket resultPacket = await resultPacketScanner.Next();
                List<EngineResult> engineResults = ParseResultPacket(resultPacket);

                List<T> results = new List<T>();
                foreach (EngineResult engineResult in engineResults)
                {
                    results.Add((T) engineResult);
                }

                return results;
            }

            return new List<T>();
        }
    }
}