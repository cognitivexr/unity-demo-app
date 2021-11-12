using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSenderNew : MonoBehaviour
{
    private EngineClient engineClient;
    
    private UnityEngine.Windows.WebCam.PhotoCapture photoCapture;
    private Resolution cameraResolution;
    private UnityEngine.Windows.WebCam.CameraParameters cameraParameters;
    
    [Header("StreamSpec")]
    public string address;
    public int port;
    
    private void CreateEngine()
    {
        StreamSpec streamSpec = new StreamSpec
        {
            engineAddress = $"{address}:{port}",
            attributes = new Attributes()
                .Set("format.width", cameraResolution.width.ToString())
                .Set("format.height", cameraResolution.height.ToString())
                .Set("format.colorMode", "4")
                .Set("format.orientation", "3")
        };
        
        JpgSendChannel sendChannel =
            new JpgSendChannel(cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight);
        FaceReceiveChannel receiveChannel = new FaceReceiveChannel();

        engineClient = new EngineClient(streamSpec, sendChannel, receiveChannel, 42);
        engineClient.Open();

        GetFaces();
    }
    
    private async void GetFaces()
    {
        while (engineClient.isConnected())
        {
            ResultReceiveChannel receiveChannel = engineClient?.GetReceiveChannel<ResultReceiveChannel>();
            if (receiveChannel == null) continue;

            var faceresult = await receiveChannel.Next<FaceEngineResult>();

        }
    }
}
