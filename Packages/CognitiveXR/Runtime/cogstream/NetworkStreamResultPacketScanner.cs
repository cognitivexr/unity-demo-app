
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CognitiveXR.CogStream
{
    public class NetworkStreamResultPacketScanner : IResultPacketScanner
    {
        private readonly NetworkStream networkStream;
        private readonly byte[] buffer = new byte[20];

        public NetworkStreamResultPacketScanner(NetworkStream stream)
        {
            networkStream = stream;
        }

        public async Task<ResultPacket> Next()
        {
            try
            {
                int readBytes = await networkStream.ReadAsync(buffer, 0, 20);

                // TODO: handle errors
                //Debug.Assert(readBytes == 20); 

                ResultPacket resultPacket = new ResultPacket
                {
                    streamId = BitConverter.ToUInt32(buffer, 0),
                    frameId = BitConverter.ToUInt32(buffer, 4),
                    seconds = BitConverter.ToUInt32(buffer, 8),
                    nanoseconds = BitConverter.ToUInt32(buffer, 12)
                };

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
}