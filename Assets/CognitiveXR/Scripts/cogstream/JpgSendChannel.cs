
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class JpgSendChannel : IFrameSendChannel
{
    private IFramePacketWriter writer;
    private int width;
    private int height;
    private GraphicsFormat graphicsFormat;
    private int quality;

    public JpgSendChannel(IFramePacketWriter writer, int width, int height, GraphicsFormat graphicsFormat, int quality = 90)
    {
        this.writer = writer;
        this.width = width;
        this.height = height;
        this.graphicsFormat = graphicsFormat;
        this.quality = quality;
    }

    public void Send(Frame frame)
    {
        byte[] jpg = ImageConversion.EncodeArrayToJPG(frame.rawFrame, graphicsFormat, (uint)width, (uint)height, 0U, quality);

        //todo: metadaten
        
        FramePacket framePacket = new FramePacket
        {
            frameId = frame.frameId,
            streamId = 99,
            timeStamp = frame.timestamp,
            metadata = new byte[0],
            data = jpg
        };
        
        writer.Write(framePacket);
    }
}
