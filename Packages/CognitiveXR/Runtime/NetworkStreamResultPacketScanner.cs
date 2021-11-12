
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class NetworkStreamResultPacketScanner : IResultPacketScanner
{
    private readonly NetworkStream networkStream;
    private readonly Thread thread;
    private bool isRunning = true;
    byte[] buffer = new byte[20];
    public OnReceivedPacket onReceivedPacket { get; set; }

    public NetworkStreamResultPacketScanner(NetworkStream stream)
    {
        networkStream = stream;
    }

    public void Shudown()
    {
        isRunning = false;
    }
    
    public async Task<ResultPacket> Next()
    {
        try
        {
            int readBytes = await networkStream.ReadAsync(buffer, 0, 20);
            
            // TODO: handle errors
            //Debug.Assert(readBytes == 20); 
            
            ResultPacket resultPacket = new ResultPacket();

            resultPacket.streamId = BitConverter.ToUInt32(buffer, 0);
            resultPacket.frameId = BitConverter.ToUInt32(buffer, 4);
            resultPacket.seconds = BitConverter.ToUInt32(buffer, 8);
            resultPacket.nanoseconds = BitConverter.ToUInt32(buffer, 12);

            UInt32 dataLenght = BitConverter.ToUInt32(buffer, 16);

            resultPacket.data = new byte[dataLenght];
            await networkStream.ReadAsync(resultPacket.data, 0, (int) dataLenght);
            
            return resultPacket;
        }
        catch (IOException e) // An error occurred when accessing the socket.
        {
            Console.WriteLine(e);
            throw;
        }
        catch (ObjectDisposedException e) // The NetworkStream is closed.
        {
            Console.WriteLine(e);
            throw;
            
        }
    }


}
