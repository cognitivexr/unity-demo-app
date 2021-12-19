using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace CognitiveXR.CogStream.Sample
{
    public class JpgSendChannel : IFrameSendChannel
    {
        private IFramePacketWriter writer;
        private uint streamId;
        
        private int width;
        private int height;
        private GraphicsFormat graphicsFormat;
        private int quality;
        private uint frameId = 0u; // auto incremented
        
        public JpgSendChannel(int width, int height, GraphicsFormat graphicsFormat = GraphicsFormat.B8G8R8A8_SRGB, int quality = 90)
        {
            this.width = width;
            this.height = height;
            this.graphicsFormat = graphicsFormat;
            this.quality = quality;
        }

        public uint? Send(Frame frame)
        {
            if (writer == null)
            {
                Debug.LogWarning("FramePacketWriter is not set. Discarding frame");
                return null;    
            }

            frame.frameId = frameId++;
            
            byte[] jpg = ImageConversion.EncodeArrayToJPG(frame.rawFrame, graphicsFormat, (uint)width, (uint)height, 0U, quality);
            
            string metadata = $"{{\"width\":{width},\"height\":{height}}}";

            FramePacket framePacket = new FramePacket
            {
                frameId = frame.frameId,
                streamId = streamId,
                seconds = (uint)DateTime.Now.Second,
                nanoseconds = (uint)DateTime.Now.Ticks, 
                metadata = System.Text.Encoding.UTF8.GetBytes(metadata),
                data = jpg
            };
            
            writer.Write(framePacket);

            return frameId;
        }

        public void SetWriter(IFramePacketWriter writer)
        {
            this.writer = writer;
        }

        public void SetStreamId(uint streamId)
        {
            this.streamId = streamId;
        }
    }
}