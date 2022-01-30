namespace CognitiveXR.CogStream
{
    public struct ResultPacket
    {
        public uint streamId;
        public uint frameId;
        public uint seconds;
        public uint nanoseconds;
        public byte[] data;
    }
}