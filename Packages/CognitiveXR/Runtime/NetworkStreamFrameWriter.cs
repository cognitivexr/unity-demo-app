using System;
using System.Net.Sockets;


namespace CognitiveXR.CogStream
{
    public class NetworkStreamFrameWriter : IFramePacketWriter
    {
        private readonly NetworkStream networkStream;

        public NetworkStreamFrameWriter(NetworkStream stream)
        {
            networkStream = stream;
        }

        public async void Write(FramePacket framePacket)
        {
            byte[] buffer = ToBuffer(framePacket);
            await networkStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private static byte[] ToBuffer(FramePacket framePacket)
        {
            byte[] byteStreamId = BitConverter.GetBytes(framePacket.streamId);
            byte[] byteFrameId = BitConverter.GetBytes(framePacket.frameId);
            byte[] byteSeconds = BitConverter.GetBytes((uint) framePacket.seconds);
            byte[] bytesNanoseconds = BitConverter.GetBytes((uint) framePacket.nanoseconds);
            byte[] byteMetadataLenght = BitConverter.GetBytes((uint) framePacket.metadata.Length);
            byte[] byteDataLenght = BitConverter.GetBytes((uint) framePacket.data.Length);

            byte[] buffer = new byte[24 + framePacket.metadata.Length + framePacket.data.Length];

            int offset = 0;
            Buffer.BlockCopy(byteStreamId, 0, buffer, offset, 4);
            offset += 4;
            Buffer.BlockCopy(byteFrameId, 0, buffer, offset, 4);
            offset += 4;
            Buffer.BlockCopy(byteSeconds, 0, buffer, offset, 4);
            offset += 4;
            Buffer.BlockCopy(bytesNanoseconds, 0, buffer, offset, 4);
            offset += 4;
            Buffer.BlockCopy(byteMetadataLenght, 0, buffer, offset, 4);
            offset += 4;
            Buffer.BlockCopy(byteDataLenght, 0, buffer, offset, 4);
            offset += 4;
            Buffer.BlockCopy(framePacket.metadata, 0, buffer, offset, framePacket.metadata.Length);
            offset += framePacket.metadata.Length;
            Buffer.BlockCopy(framePacket.data, 0, buffer, offset, framePacket.data.Length);

            return buffer;
        }
    }
}