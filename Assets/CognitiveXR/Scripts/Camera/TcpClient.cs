using System.Collections;
using System.Collections.Generic;
#if UNITY_UWP
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
#endif
using UnityEngine;

public class TcpClient
{
#if UNITY_UWP
    private Stream stream;
    private StreamWriter streamWriter;
#endif
    
    public TcpClient(string ip, int port)
    {
#if UNITY_UWP
        Task.Run(async () => {
            StreamSocket socket = new StreamSocket();
            await socket.ConnectAsync(new HostName(IP),port.ToString());
            stream = socket.OutputStream.AsStreamForWrite();
            writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(socket.InputStream.AsStreamForRead());
            try
            {
                string data = await reader.ReadToEndAsync();
            }
            catch (Exception) { }
            writer = null;
        });
#endif
    }
    
    public void SendImage(byte[] image)
    {
#if UNITY_UWP
        if (stream != null) Task.Run(async () =>
        {
            await stream.WriteAsync(image, 0, image.Length);
            await stream.FlushAsync();
        });
#endif
    }

}
