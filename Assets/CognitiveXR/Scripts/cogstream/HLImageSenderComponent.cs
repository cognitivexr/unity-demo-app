using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using HoloLensCameraStream;
using TMPro;
using CognitiveXR.CogStream;
using Microsoft.MixedReality.Toolkit.Utilities;
using Unity.XRTools.Rendering;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.WSA;
using UnityEngine.XR.WSA;
using CapturePixelFormat = HoloLensCameraStream.CapturePixelFormat;
#if WINDOWS_UWP
using global::Windows.Perception.Spatial;
#endif // WINDOWS_UWP


public class HLImageSenderComponent : MonoBehaviour
{
    // engine
    private EngineClient engineClient;

    private IntPtr spatialCoordinateSystemPtr;
    private HoloLensCameraStream.VideoCapture videoCapture;
    private byte[] latestImageBytes;
    private HoloLensCameraStream.Resolution resolution;
    private ResultReceiveChannel receiveChannel;
    private string engineAddress;
    
    [Header("Debug")]
    [SerializeField] private bool ShowDebugCameraImage = true;
    
    private GameObject _picture;
    private Renderer _pictureRenderer;
    private Texture2D _pictureTexture;
    
    public class SampleStruct
    {
        public HoloLensCameraStream.Resolution resolution;
        public Matrix4x4 camera2WorldMatrix, projectionMatrix;
        public Pose cameraPose;
    }

    // cached spatial info of last send frame
    private SampleStruct spatialInfo;
    
    // used to only send one frame at a time
    private bool receivedNewFrame = true;

    public void Launcher(string engineAddress)
    {
        this.engineAddress = engineAddress;
        
        CreateCamera();
    }

    private void Start()
    {
        CreateCamera();
    }

    private void OnDestroy()
    {
#if WINDOWS_UWP
        if (videoCapture != null)
        {
            videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
            videoCapture.Dispose();
        }
#endif
    }
    
    private void CreateCamera()
    {
        spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
#if WINDOWS_UWP
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureResourceCreatedCallback);
#endif
        if (ShowDebugCameraImage)
        {
            _picture = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _pictureRenderer = _picture.GetComponent<Renderer>() as Renderer;
            _pictureRenderer.material = new Material(Shader.Find("AR/HolographicImageBlend"));
        }
    }
    
    private void OnVideoCaptureResourceCreatedCallback(HoloLensCameraStream.VideoCapture captureobject)
    {
        videoCapture = captureobject;
#if WINDOWS_UWP
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

        if (ShowDebugCameraImage)
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() => { _pictureTexture =
 new Texture2D(resolution.width, resolution.height, TextureFormat.BGRA32, false); }, false);
        }

        videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
#endif
    }
    
    void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (result.success == false)
        {
            Debug.LogError("Could not start video mode.");
            return;
        }

        // create engine and connect
        CreateEngineClient();
    }

    private void CreateEngineClient()
    {
        StreamSpec streamSpec = new StreamSpec
        {
            engineAddress = engineAddress,
            attributes = new Attributes()
                .Set("format.width", resolution.width.ToString())
                .Set("format.height", resolution.height.ToString())
                .Set("format.colorMode", "4")
                .Set("format.orientation", "3")
        };

        JpgSendChannel sendChannel = new JpgSendChannel(resolution.width, resolution.height);

        engineClient = new EngineClient(streamSpec, sendChannel, receiveChannel);
        engineClient.Open();
    }

    public EngineClient GetEngine()
    {
        return engineClient;
    }

    public SampleStruct GetSample()
    {
        return spatialInfo;
    }

    public void SetReceiveChannel(ResultReceiveChannel receiveChannel)
    {
        this.receiveChannel = receiveChannel;
    }

    public void SetReceivedNewFrame(bool newValue)
    {
        lock (this)
        {
            receivedNewFrame = newValue;
        }
    }

    void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        // only send one frame at a time and wait for an answer before sending the next one
         lock (this)
         {
             if(receivedNewFrame == false)
             {
                 return;
             }
         }
#if WINDOWS_UWP
         if(latestImageBytes == null || latestImageBytes.Length < sample.dataLength)
            latestImageBytes = new byte[sample.dataLength];
         
         sample.CopyRawImageDataIntoBuffer(latestImageBytes);

        // Get the cameraToWorldMatrix and projectionMatrix
        if(!sample.TryGetCameraToWorldMatrix(out float[] camera2WorldMatrixArray) || !sample.TryGetProjectionMatrix(out float[] projectionMatrixArray))
            return;

        sample.Dispose();

        Matrix4x4 camera2WorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(camera2WorldMatrixArray);
        Matrix4x4 projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(projectionMatrixArray);
#else
         Matrix4x4 camera2WorldMatrix = new Matrix4x4();
         Matrix4x4 projectionMatrix = new Matrix4x4();
#endif

        // cache the camera
         SampleStruct s = new SampleStruct
         {
             resolution = resolution,
             camera2WorldMatrix = camera2WorldMatrix,
             projectionMatrix = projectionMatrix,
             cameraPose = new Pose(Camera.main.transform.position, Camera.main.transform.rotation)
         };

         Frame frame = new Frame
         {
             timestamp = DateTime.Now,
             rawFrame = latestImageBytes,
             height = resolution.height,
             width = resolution.width,
         };

         JpgSendChannel jpgSendChannel = engineClient.GetSendChannel<JpgSendChannel>();
         uint? frameId = jpgSendChannel?.Send(frame);

         if (frameId.HasValue)
         {
             SetReceivedNewFrame(false);
             spatialInfo = s;
         }
        
         if (ShowDebugCameraImage)
         {
             UnityEngine.WSA.Application.InvokeOnAppThread(() =>
             {
                
                 // Upload bytes to texture
                 _pictureTexture.LoadRawTextureData(latestImageBytes);
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
    }

}
