using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;


/// <summary>
/// For debug purpose
/// </summary>
public class ImageSenderComponent : MonoBehaviour
{
    private EngineClient engineClient;
    private uint frameId = 0;
    
    private UnityEngine.Windows.WebCam.PhotoCapture photoCapture;
    private Resolution cameraResolution;
    private UnityEngine.Windows.WebCam.CameraParameters cameraParameters;
    
    [Header("StreamSpec")]
    public string address;
    public int port;

    private Texture2D Picture;
    
    private void Start()
    {
        CreateCamera();
    }
    
    private void Update()
    {
        ResultReceiveChannel receiveChannel = engineClient?.GetReceiveChannel<ResultReceiveChannel>();
        if (receiveChannel == null) return;
        
        while (receiveChannel.TryDequeue(out EngineResult engineResult))
        {
            Debug.Log(engineResult.result);
        }
    }
    
    private void CreateCamera()
    {
        cameraResolution = UnityEngine.Windows.WebCam.VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        float cameraFramerate = UnityEngine.Windows.WebCam.VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution)
            .OrderByDescending((fps) => fps).First();

        UnityEngine.Windows.WebCam.PhotoCapture.CreateAsync(false, delegate(UnityEngine.Windows.WebCam.PhotoCapture captureObject)
        {
            photoCapture = captureObject;
            cameraParameters = new UnityEngine.Windows.WebCam.CameraParameters
            {
                hologramOpacity = 0.0f,
                cameraResolutionWidth = cameraResolution.width,
                cameraResolutionHeight = cameraResolution.height,
                pixelFormat = UnityEngine.Windows.WebCam.CapturePixelFormat.BGRA32
            };
            
            // Activate the camera
            photoCapture.StartPhotoModeAsync(cameraParameters, delegate(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result)
            {
                Debug.Log("camera is ready");
                
                CreateEngineClient();
            });
        });
    }

    private void CreateEngineClient()
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
        DebugReceiveChannel receiveChannel = new DebugReceiveChannel();

        engineClient = new EngineClient(streamSpec, sendChannel, receiveChannel);
        engineClient.Open();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 600));

        if (GUILayout.Button("Send image"))
        {
            photoCapture?.TakePhotoAsync(OnCapturedPhotoToMemory);
        }

        if (Picture != null)
        {
            GUI.DrawTexture(new Rect(10,100, 300, 300) , Picture, ScaleMode.ScaleToFit);
        }

        GUILayout.EndArea();
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        List<byte> imageBufferList = new List<byte>();
        photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
        
        //todo: invert image
        int stride = 4;
        float denominator = 1.0f / 255.0f;
        List<Color> colorArray = new List<Color>();
        for (int i = imageBufferList.Count - 1; i >= 0; i -= stride)
        {
            float a = (int)(imageBufferList[i - 0]) * denominator;
            float r = (int)(imageBufferList[i - 1]) * denominator;
            float g = (int)(imageBufferList[i - 2]) * denominator;
            float b = (int)(imageBufferList[i - 3]) * denominator;

            colorArray.Add(new Color(r, g, b, a));
        }
        
        Picture = new Texture2D(cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight, TextureFormat.RGBA32, false);
        Picture.SetPixels(colorArray.ToArray());
        Picture.Apply();

        Frame frame = new Frame
        {
            timestamp = DateTime.Now,
            rawFrame = imageBufferList.ToArray(),
            height = cameraParameters.cameraResolutionHeight,
            width = cameraParameters.cameraResolutionWidth,
            frameId = frameId++
        };

        JpgSendChannel jpgSendChannel = engineClient.GetSendChannel<JpgSendChannel>();
        jpgSendChannel?.Send(frame);
    }
}
