
namespace CognitiveXR.CogStream
{
    public struct FramePacket
    {
        public uint streamId;
        public uint frameId;
        public uint seconds;
        public uint nanoseconds;
        public byte[] metadata;
        public byte[] data;
    }
}