
namespace CognitiveXR.CogStream
{
    public interface IFrameSendChannel
    {
        /// <summary>
        /// Sends a frame and returns the the frame id if successful
        /// </summary>
        /// <param name="frame"></param>
        /// <returns>frameId if successful</returns>
        uint? Send(Frame frame);

        /// <summary>
        /// Sets the FramePacketWriter
        /// </summary>
        /// <param name="writer"></param>
        void SetWriter(IFramePacketWriter writer);

        /// <summary>
        /// Sets the stream id of the frame channel
        /// </summary>
        /// <param name="streamId"></param>
        void SetStreamId(uint streamId);
    }
}