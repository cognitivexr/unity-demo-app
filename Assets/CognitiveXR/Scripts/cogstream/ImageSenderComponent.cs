using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using CognitiveXR.CogStream;

/// <summary>
/// For debug purpose
/// </summary>
public class ImageSenderComponent : MonoBehaviour
{
    private EngineClient engineClient;
    private uint frameId = 0;
    
    private PhotoCapture photoCapture;
    private Resolution cameraResolution;
    private CameraParameters cameraParameters;
    
    [Header("StreamSpec")]
    public string address;
    public int port;

    private Texture2D Picture;
    [SerializeField] private TextMeshProUGUI textfield;
    private DateTime time;
    
    private void Start()
    {
        CreateCamera();
    }
    
    private void CreateCamera()
    {
        cameraResolution =VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution)
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

        JpgSendChannel sendChannel = new JpgSendChannel(cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight);
        DebugReceiveChannel receiveChannel = new DebugReceiveChannel();

        engineClient = new EngineClient(streamSpec, sendChannel, receiveChannel, 42);
        engineClient.Open();

        Task.Run(UpdateFaceEngineResults);
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        Task.Run(async () =>
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
            /*
            Picture = new Texture2D(cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight, TextureFormat.RGBA32, false);
            Picture.SetPixels(colorArray.ToArray());
            Picture.Apply();
            */

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
            time = DateTime.Now;
            /*
            for (int i = 0; i < 1000; i++)
            {
                await Task.Delay(66);
                jpgSendChannel?.Send(frame);
                time = DateTime.Now;
            }
            */
        });
    }
    
    private async void UpdateFaceEngineResults()
    {
        DebugReceiveChannel receiveChannel = engineClient?.GetReceiveChannel<DebugReceiveChannel>();
        if (receiveChannel == null) return;

        while (true)
        {
            List<EngineResult> faceEngineResults = await receiveChannel.Next<EngineResult>();
            foreach (EngineResult faceEngineResult in faceEngineResults)
            {
                Debug.Log(faceEngineResult.result);
                Debug.Log((DateTime.Now - time).Milliseconds);
            }
        }
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


}
