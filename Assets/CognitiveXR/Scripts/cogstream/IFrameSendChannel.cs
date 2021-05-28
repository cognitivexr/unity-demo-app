
public interface IFrameSendChannel
{
    void Send(Frame frame);

    void SetWriter(IFramePacketWriter writer);
}
