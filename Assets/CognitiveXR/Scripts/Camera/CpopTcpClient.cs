using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if WINDOWS_UWP
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
#endif

using UnityEngine;

public class CpopTcpClient
{
#if WINDOWS_UWP
    private Stream stream;
    private StreamWriter streamWriter;
#endif
    
    public CpopTcpClient(string ip, int port)
    {
#if WINDOWS_UWP
        Task.Run(async () => {
            StreamSocket socket = new StreamSocket();
            await socket.ConnectAsync(new HostName(ip),port.ToString());
            stream = socket.OutputStream.AsStreamForWrite();
            streamWriter = new StreamWriter(stream);
            StreamReader reader = new StreamReader(socket.InputStream.AsStreamForRead());
            try
            {
                string data = await reader.ReadToEndAsync();
            }
            catch (Exception) { }
            streamWriter = null;
        });
#endif
    }
    
    public void SendImage(byte[] image)
    {
        int length = image.Length;
        byte[] encodedLength = BitConverter.GetBytes(length);

#if WINDOWS_UWP
        if (stream != null) Task.Run(async () =>
        {
            await stream.WriteAsync(encodedLength, 0, 4);
            await stream.WriteAsync(image, 0, image.Length);
            await stream.FlushAsync();
        });
#endif
    }

}
