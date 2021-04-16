
using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    private EngineClient engineClient;

    public string address;
    public int port;
    
    private void Start()
    {
        StreamSpec streamSpec = new StreamSpec
        {
            engineAddress = $"{address}:{port}",
            attributes = new Attributes()
        };

        engineClient = new EngineClient(streamSpec);
        engineClient.Open();

        FramePacket framePacket = new FramePacket
        {
            streamId = 0,
            frameId = 0,
            timeStamp = DateTime.Now,
            metadata = new byte[0],
            data = new byte[0]
        };
        
        engineClient.Request(framePacket);

        Debug.Log("ok");
    }
}
