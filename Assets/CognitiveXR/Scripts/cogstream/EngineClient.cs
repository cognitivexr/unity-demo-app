using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class EngineClient
{
    private StreamSpec streamSpec;
    private NetworkStream networkStream;
    private IFramePacketWriter frameWriter; // todo: delete
    private IResultPacketScanner packetScanner; // todo: delete

    private IResultReceiveChannel resultReceiveChannel;

    public EngineClient(StreamSpec streamSpec)
    {
        this.streamSpec = streamSpec;
    }

    public void Open()
    {
        string[] parts = streamSpec.engineAddress.Split(':');
       
        IPAddress ipAddress = IPAddress.Parse(parts[0]);
        IPEndPoint endPoint = new IPEndPoint(ipAddress, int.Parse(parts[1]));
        TcpClient client = new TcpClient();
        {
            client.Connect(endPoint);
            networkStream = client.GetStream();
            {
                string json = JsonUtility.ToJson(streamSpec);
                byte[] jsonAsBytes = Encoding.UTF8.GetBytes(json);
                byte[] lengthInByte = BitConverter.GetBytes(jsonAsBytes.Length);
                
                byte[] data = new byte[4 + jsonAsBytes.Length];
                Buffer.BlockCopy(lengthInByte, 0, data, 0, lengthInByte.Length);
                Buffer.BlockCopy(jsonAsBytes, 0, data, 4, jsonAsBytes.Length);
                
                networkStream.Write(data,0, data.Length);
                networkStream.Flush();

                frameWriter = new NetworkStreamFrameWriter(networkStream);
                packetScanner = new NetworkStreamResultPacketScanner(networkStream);
            }
        }
    }
    

    
    
    public EngineResult DebugRequest(Frame frame)
    {
        frameWriter.Write(framePacket);
        ResultPacket result = packetScanner.Next();
        
    }


    
}
