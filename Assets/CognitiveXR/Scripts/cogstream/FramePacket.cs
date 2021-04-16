using System;

public struct FramePacket
{
    public uint streamId;
    public uint frameId;
    public DateTime timeStamp;
    public byte[] metadata;
    public byte[] data;

    public byte[] ToBytes()
    {
        byte[] byteStreamId = BitConverter.GetBytes(streamId);
        byte[] byteFrameId = BitConverter.GetBytes(frameId);
        byte[] byteSeconds = BitConverter.GetBytes((uint)timeStamp.Second);
        byte[] bytesNanoseconds = BitConverter.GetBytes((uint)timeStamp.Millisecond);
        byte[] byteMetadataLenght = BitConverter.GetBytes((uint)metadata.Length);
        byte[] byteDataLenght = BitConverter.GetBytes((uint) data.Length);

        byte[] buffer = new byte[24 + metadata.Length + data.Length];

        int offset = 0;
        Buffer.BlockCopy( byteStreamId, 0, buffer, offset, 4);
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
        Buffer.BlockCopy(metadata, 0, buffer, offset, metadata.Length);
        offset += metadata.Length;
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);

        return buffer;
    }
}