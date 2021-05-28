using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HoloLensCameraStream;
using TMPro;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.XR.WSA;
#if WINDOWS_UWP
using global::Windows.Perception.Spatial;
#endif // WINDOWS_UWP


public class HLImageSenderComponent : MonoBehaviour
{
    // engine
    private EngineClient engineClient;
    private uint frameId = 0;

    private IntPtr spatialCoordinateSystemPtr;
    private HoloLensCameraStream.VideoCapture videoCapture;
    private byte[] latestImageBytes;
    private HoloLensCameraStream.Resolution resolution;
    
    [Header("StreamSpec")] 
    public string address;
    public int port;

    [SerializeField] private TextMeshProUGUI textfield;

#if WINDOWS_UWP
    private void Start()
    {
        CreateCamera();
    }
    
    private void OnDestroy()
    {
        if (videoCapture != null)
        {
            videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
            videoCapture.Dispose();
        }
    }

    private void Update()
    {
        ResultReceiveChannel receiveChannel = engineClient?.GetReceiveChannel<ResultReceiveChannel>();
        if (receiveChannel == null) return;

        while (receiveChannel.TryDequeue<FaceEngineResult>(out FaceEngineResult engineResult))
        {
            if (engineResult.emotions.Count > 0)
            {
                textfield.text = engineResult.emotions[0].label;
            }
        }
    }

    private void CreateCamera()
    {
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureResourceCreatedCallback);

        spatialCoordinateSystemPtr =
 Microsoft.MixedReality.Toolkit.WindowsMixedReality.WindowsMixedRealityUtilities.UtilitiesProvider.ISpatialCoordinateSystemPtr;
    }
    
    private void OnVideoCaptureResourceCreatedCallback(HoloLensCameraStream.VideoCapture captureobject)
    {
        videoCapture = captureobject;
        
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystemPtr(spatialCoordinateSystemPtr);

        resolution = CameraStreamHelper.Instance.GetLowestResolution();
        float frameRate = 15;//CameraStreamHelper.Instance.GetLowestFrameRate(resolution);
        videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;
        
        HoloLensCameraStream.CameraParameters cameraParams = new HoloLensCameraStream.CameraParameters();
        cameraParams.cameraResolutionHeight = resolution.height;
        cameraParams.cameraResolutionWidth = resolution.width;
        cameraParams.frameRate = Mathf.RoundToInt(frameRate);
        cameraParams.pixelFormat = CapturePixelFormat.BGRA32;
        cameraParams.rotateImage180Degrees = false; //If your image is upside down, remove this line.
        cameraParams.enableHolograms = false;
        cameraParams.enableVideoStabilization = false;
        cameraParams.recordingIndicatorVisible = false;
        
        videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
    }
    
    void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (result.success == false)
        {
            Debug.LogWarning("Could not start video mode.");
            return;
        }

        // create engine and connect
        CreateEngineClient();
    }

    private void CreateEngineClient()
    {
        StreamSpec streamSpec = new StreamSpec
        {
            engineAddress = $"{address}:{port}",
            attributes = new Attributes()
                .Set("format.width", resolution.width.ToString())
                .Set("format.height", resolution.height.ToString())
                .Set("format.colorMode", "4")
                .Set("format.orientation", "3")
        };

        JpgSendChannel sendChannel =
            new JpgSendChannel(resolution.width, resolution.height);
        FaceReceiveChannel faceReceiveChannel = new FaceReceiveChannel();

        engineClient = new EngineClient(streamSpec, sendChannel, faceReceiveChannel);
        engineClient.Open();
    }
    
    void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        if (latestImageBytes == null || latestImageBytes.Length < sample.dataLength)
        {
            latestImageBytes = new byte[sample.dataLength];
        }
        sample.CopyRawImageDataIntoBuffer(latestImageBytes);

        Frame frame = new Frame
        {
            timestamp = DateTime.Now,
            rawFrame = latestImageBytes,
            height = resolution.height,
            width = resolution.width,
            frameId = frameId++
        };

        JpgSendChannel jpgSendChannel = engineClient.GetSendChannel<JpgSendChannel>();
        jpgSendChannel?.Send(frame);
    }
#endif // WINDOWS_UWP
}
