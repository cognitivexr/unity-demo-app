using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

public class NetworkStreamFrameWriter : IFramePacketWriter
{
    private Thread thread;
    private EventWaitHandle eventWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
    private readonly NetworkStream networkStream;
    private ConcurrentQueue<FramePacket> framePacketQueue = new ConcurrentQueue<FramePacket>();
    private bool isRunning = true;
    public NetworkStreamFrameWriter(NetworkStream stream)
    {
        networkStream = stream;
        thread = new Thread(Run);
        thread.Start();
    }

    private void Run()
    {
        eventWaitHandle.Reset();
        eventWaitHandle.WaitOne();

        while (isRunning)
        {
            // wait for data to send 
            eventWaitHandle.Reset();

            while (framePacketQueue.TryDequeue(out FramePacket framePacket))
            {
                byte[] buffer = ToBuffer(framePacket);
                networkStream.Write(buffer, 0, buffer.Length);
            }
        }
    }

    public void Shutdown()
    {
        isRunning = false;
        eventWaitHandle.Set();
    }

    public void Write(FramePacket framePacket)
    {
        framePacketQueue.Enqueue(framePacket);
        eventWaitHandle.Set();
    }

    private static byte[] ToBuffer(FramePacket framePacket)
    {
        byte[] byteStreamId = BitConverter.GetBytes(framePacket.streamId);
        byte[] byteFrameId = BitConverter.GetBytes(framePacket.frameId);
        byte[] byteSeconds = BitConverter.GetBytes((uint)framePacket.seconds);
        byte[] bytesNanoseconds = BitConverter.GetBytes((uint)framePacket.nanoseconds);
        byte[] byteMetadataLenght = BitConverter.GetBytes((uint)framePacket.metadata.Length);
        byte[] byteDataLenght = BitConverter.GetBytes((uint) framePacket.data.Length);

        byte[] buffer = new byte[24 + framePacket.metadata.Length + framePacket.data.Length];

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
        Buffer.BlockCopy(framePacket.metadata, 0, buffer, offset, framePacket.metadata.Length);
        offset += framePacket.metadata.Length;
        Buffer.BlockCopy(framePacket.data, 0, buffer, offset, framePacket.data.Length);

        return buffer;
    }
}
