using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class JpgSendChannel : IFrameSendChannel
{
    private IFramePacketWriter writer;
    private int width;
    private int height;
    private GraphicsFormat graphicsFormat;
    private int quality;

    public JpgSendChannel(IFramePacketWriter writer, int width, int height, GraphicsFormat graphicsFormat = GraphicsFormat.B8G8R8A8_SRGB, int quality = 90)
    {
        this.writer = writer;
        this.width = width;
        this.height = height;
        this.graphicsFormat = graphicsFormat;
        this.quality = quality;
    }
    
    public JpgSendChannel(int width, int height, GraphicsFormat graphicsFormat = GraphicsFormat.B8G8R8A8_SRGB, int quality = 90)
    {
        this.width = width;
        this.height = height;
        this.graphicsFormat = graphicsFormat;
        this.quality = quality;
    }

    public void Send(Frame frame)
    {
        byte[] jpg = ImageConversion.EncodeArrayToJPG(frame.rawFrame, graphicsFormat, (uint)width, (uint)height, 0U, quality);
        
        string metadata = $"{{\"width\":{width},\"height\":{height}}}";

        FramePacket framePacket = new FramePacket
        {
            frameId = frame.frameId,
            streamId = 42,
            seconds = (uint)DateTime.Now.Second,
            nanoseconds = (uint)DateTime.Now.Ticks, 
            metadata = System.Text.Encoding.UTF8.GetBytes(metadata),
            data = jpg
        };
        
        writer.Write(framePacket);
    }

    public void SetWriter(IFramePacketWriter writer)
    {
        this.writer = writer;
    }
}
