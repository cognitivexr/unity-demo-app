using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CognitiveXR.CogStream;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.WebCam;

/// <summary>
/// For debug purpose
/// </summary>
public class ImageSenderComponent : MonoBehaviour
{
    private EngineClient engineClient;

    private PhotoCapture photoCapture;
    private Resolution cameraResolution;
    private CameraParameters cameraParameters;
    private CancellationTokenSource cancellationTokenSource;
    
    [Header("StreamSpec")]
    public string address;
    public int port;

    private Texture2D Picture;
    [SerializeField] private TextMeshProUGUI textfield;
    
    public delegate void OnEmotionDetectedDelegate(BoundingBox.BoundingBoxInfo info);
    public OnEmotionDetectedDelegate OnEmotionDetected;

    //private Object _locker = new Object();
    private bool receivedNewFrame = true;
    
    private void Start()
    {
        CreateCamera();
    }
    
    private void CreateCamera()
    {
        cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

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
                if (!result.success)
                {
                    Debug.LogError("Failed to start camera");
                    return;
                }
                
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
        FaceReceiveChannel receiveChannel = new FaceReceiveChannel();
        
        engineClient = new EngineClient(streamSpec, sendChannel, receiveChannel, 42);
        engineClient.Open();

        cancellationTokenSource = new CancellationTokenSource();
        Task.Run(UpdateFaceEngineResults, cancellationTokenSource.Token);
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        
        lock (this)
        {
            if(receivedNewFrame == false)
            {
                return;
            }
        }
        
        
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
        };

        JpgSendChannel jpgSendChannel = engineClient.GetSendChannel<JpgSendChannel>();
        uint? frameId = jpgSendChannel?.Send(frame);
        
        if (frameId.HasValue)
        {
            lock (this)
            {
                receivedNewFrame = false;
            }
        }

    }
    
    private async void UpdateFaceEngineResults()
    {
        FaceReceiveChannel receiveChannel = engineClient?.GetReceiveChannel<FaceReceiveChannel>();
        if (receiveChannel == null) return;

        while (!cancellationTokenSource.IsCancellationRequested)
        {
            List<FaceEngineResult> faceEngineResults = await receiveChannel.Next<FaceEngineResult>();

            lock (this)
            {
                receivedNewFrame = true;
            }

            foreach (FaceEngineResult engineResult in faceEngineResults)
            {
                if (engineResult.emotions.Count > 0)
                {
                    var DominantEmotion = engineResult.emotions.Select(x => (x.probability, x)).Max().x.label;
                    OnEmotionDetected(new BoundingBox.BoundingBoxInfo()
                    {
                        Bounds = new List<Vector3>() {Vector3.zero, Vector3.zero, Vector3.zero,Vector3.zero},
                        text = engineResult.emotions.Select(x => (x.probability, x)).Max().x.label,
                        frameId = engineResult.frameId,
                        cameraPose = new Pose()
                    });
                }
            }
        }
        
        Debug.Log("UpdateFaceEngineResults finished");
    }
    
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 600));

        if (GUILayout.Button("Send image"))
        { 
            photoCapture?.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        
        if (GUILayout.Button("Cancel"))
        { 
            cancellationTokenSource.Cancel();
        }

        if (Picture != null)
        {
            GUI.DrawTexture(new Rect(10,100, 300, 300) , Picture, ScaleMode.ScaleToFit);
        }

        GUILayout.EndArea();
    }


}
