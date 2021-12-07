
namespace CognitiveXR.CogStream
{
    public interface IFrameSendChannel
    {
        void Send(Frame frame);

        void SetWriter(IFramePacketWriter writer);

        void SetStreamId(uint streamId);
    }
}