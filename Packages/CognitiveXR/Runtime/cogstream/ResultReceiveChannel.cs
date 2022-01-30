using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveXR.CogStream
{
    
    /// <summary>
    /// ResultReceiveChannel are used by the EngineClient to return results form the engine
    /// </summary>
    public abstract class ResultReceiveChannel
    {
        private IResultPacketScanner resultPacketScanner;

        /// <summary>
        /// Parses the result packet proper
        /// </summary>
        /// <param name="resultPacket">list of parsed engine results</param>
        /// <returns></returns>
        protected abstract List<EngineResult> ParseResultPacket(ResultPacket resultPacket);

        /// <summary>
        /// Set the packet scanner
        /// </summary>
        /// <param name="inResultPacketScanner"></param>
        public void SetResultPacketScanner(IResultPacketScanner inResultPacketScanner)
        {
            this.resultPacketScanner = inResultPacketScanner;
        }
        
        /// <summary>
        /// Returns a engine results 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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