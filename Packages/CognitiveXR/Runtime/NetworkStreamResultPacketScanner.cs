
using System;
using System.Net.Sockets;
using System.Threading;

public class NetworkStreamResultPacketScanner : IResultPacketScanner
{
    private readonly NetworkStream networkStream;
    private readonly Thread thread;
    private bool isRunning = true;
    public OnReceivedPacket onReceivedPacket { get; set; }

    public NetworkStreamResultPacketScanner(NetworkStream stream)
    {
        networkStream = stream;
        thread = new Thread(Run);
        thread.Start();
    }

    private void Run()
    {
        while (isRunning)
        {
            ResultPacket resultPacket = Next();
            onReceivedPacket?.Invoke(resultPacket);
        }
    }

    public void Shudown()
    {
        isRunning = false;
    }
    
    public ResultPacket Next()
    {
        byte[] buffer = new byte[20];

        int readBytes = networkStream.Read(buffer, 0, 20);
        
        // TODO: handle errors
        //Debug.Assert(readBytes == 20); 
        
        ResultPacket resultPacket = new ResultPacket();

        resultPacket.streamId = BitConverter.ToUInt32(buffer, 0);
        resultPacket.frameId = BitConverter.ToUInt32(buffer, 4);
        resultPacket.seconds = BitConverter.ToUInt32(buffer, 8);
        resultPacket.nanoseconds = BitConverter.ToUInt32(buffer, 12);

        UInt32 dataLenght = BitConverter.ToUInt32(buffer, 16);

        resultPacket.data = new byte[dataLenght];
        networkStream.Read(resultPacket.data, 0, (int) dataLenght);

        return resultPacket;
    }


}
