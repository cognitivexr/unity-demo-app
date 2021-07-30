using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HoloLensCameraStream;
using TMPro;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.XR.WSA;
using CapturePixelFormat = HoloLensCameraStream.CapturePixelFormat;
#if WINDOWS_UWP
using global::Windows.Perception.Spatial;
using UnityEngine.Windows.WebCam;

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

    private GameObject _picture;
    private Renderer _pictureRenderer;
    private Texture2D _pictureTexture;
    
    private class SampleStruct
    {
        public float[] camera2WorldMatrix, projectionMatrix;
        public byte[] data;
    }
    
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

        spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

        _picture = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _pictureRenderer = _picture.GetComponent<Renderer>() as Renderer;
        _pictureRenderer.material = new Material(Shader.Find("AR/HolographicImageBlend"));
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
        
        UnityEngine.WSA.Application.InvokeOnAppThread(() => { _pictureTexture = new Texture2D(resolution.width, resolution.height, TextureFormat.BGRA32, false); }, false);


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
         if(latestImageBytes == null || latestImageBytes.Length < sample.dataLength)
            latestImageBytes = new byte[sample.dataLength];

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

        SampleStruct s = new SampleStruct();
        s.data = latestImageBytes;

        // Get the cameraToWorldMatrix and projectionMatrix
        if(!sample.TryGetCameraToWorldMatrix(out s.camera2WorldMatrix) || !sample.TryGetProjectionMatrix(out s.projectionMatrix))
            return;

        sample.Dispose();

        Matrix4x4 camera2WorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(s.camera2WorldMatrix);
        Matrix4x4 projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(s.projectionMatrix);

        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {

            // Upload bytes to texture
            _pictureTexture.LoadRawTextureData(s.data);
            _pictureTexture.wrapMode = TextureWrapMode.Clamp;
            _pictureTexture.Apply();

            // Set material parameters
            _pictureRenderer.sharedMaterial.SetTexture("_MainTex", _pictureTexture);
            _pictureRenderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", camera2WorldMatrix.inverse);
            _pictureRenderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);
            _pictureRenderer.sharedMaterial.SetFloat("_VignetteScale", 0f);

            Vector3 inverseNormal = -camera2WorldMatrix.GetColumn(2);
            // Position the canvas object slightly in front of the real world web camera.
            Vector3 imagePosition = camera2WorldMatrix.GetColumn(3) - camera2WorldMatrix.GetColumn(2);

            _picture.transform.position = imagePosition;
            _picture.transform.rotation = Quaternion.LookRotation(inverseNormal, camera2WorldMatrix.GetColumn(1));

        }, false);
    }
#else
    
#endif // WINDOWS_UWP


}
