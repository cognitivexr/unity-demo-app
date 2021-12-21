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
    private CancellationTokenSource cancellationTokenSource;
    
    [Header("StreamSpec")] 
    public string address;
    public int port;
    
    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI textfield;

    [SerializeField] private bool ShowDebugCameraImage = true;
    
    private GameObject _picture;
    private Renderer _pictureRenderer;
    private Texture2D _pictureTexture;
    
    private class SampleStruct
    {
        public Matrix4x4 camera2WorldMatrix, projectionMatrix;
        public Pose cameraPose;
    }

    private Dictionary<uint, SampleStruct> spatialInfo = new Dictionary<uint, SampleStruct>();
        
    public delegate void OnEmotionDetectedDelegate(EmotionBox.EmotionInfo info);
    public OnEmotionDetectedDelegate OnEmotionDetected;
    
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
    
    private void CreateCamera()
    {
        spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureResourceCreatedCallback);

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
            engineAddress = $"{address}:{port}",
            attributes = new Attributes()
                .Set("format.width", resolution.width.ToString())
                .Set("format.height", resolution.height.ToString())
                .Set("format.colorMode", "4")
                .Set("format.orientation", "3")
        };

        JpgSendChannel sendChannel = new JpgSendChannel(resolution.width, resolution.height);
        FaceReceiveChannel faceReceiveChannel = new FaceReceiveChannel();

        engineClient = new EngineClient(streamSpec, sendChannel, faceReceiveChannel);
        engineClient.Open();

        cancellationTokenSource = new CancellationTokenSource();
        Task.Run(UpdateFaceEngineResults, cancellationTokenSource.Token);
    }
    
    private async void UpdateFaceEngineResults()
    {
        FaceReceiveChannel receiveChannel = engineClient?.GetReceiveChannel<FaceReceiveChannel>();
        if (receiveChannel == null) return;

        while (!cancellationTokenSource.IsCancellationRequested)
        {
            List<FaceEngineResult> faceEngineResults = await receiveChannel.Next<FaceEngineResult>();
            foreach (FaceEngineResult engineResult in faceEngineResults)
            {
                if (engineResult.emotions.Count > 0)
                {

                    SampleStruct s = spatialInfo[engineResult.frameId];

                    Vector3 pos1 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                        resolution,
                        new Vector2(engineResult.face[0], engineResult.face[1]));
                    Vector3 pos2 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                        resolution,
                        new Vector2(engineResult.face[0], engineResult.face[1] + engineResult.face[3]));
                    Vector3 pos4 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                        resolution,
                        new Vector2(engineResult.face[0] + engineResult.face[2], engineResult.face[1]));
                    Vector3 pos3 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                        resolution,
                        new Vector2(engineResult.face[0] + engineResult.face[2],
                            engineResult.face[1] + engineResult.face[3]));

                    OnEmotionDetected(new EmotionBox.EmotionInfo()
                    {
                        Bounds = new List<Vector3>()
                        {
                            pos1, pos2, pos3, pos4
                        },
                        DominantEmotion = engineResult.emotions.Select(x => (x.probability, x)).Max().x.label,
                        frameId = engineResult.frameId,
                        cameraPose = s.cameraPose
                    });
                }
            }
        }
    }
    
    void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
         if(latestImageBytes == null || latestImageBytes.Length < sample.dataLength)
            latestImageBytes = new byte[sample.dataLength];
         
         sample.CopyRawImageDataIntoBuffer(latestImageBytes);

        // Get the cameraToWorldMatrix and projectionMatrix
        if(!sample.TryGetCameraToWorldMatrix(out float[] camera2WorldMatrixArray) || !sample.TryGetProjectionMatrix(out float[] projectionMatrixArray))
            return;

        sample.Dispose();

        Matrix4x4 camera2WorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(camera2WorldMatrixArray);
        Matrix4x4 projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(projectionMatrixArray);

        SampleStruct s = new SampleStruct
        {
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
            spatialInfo.Add(frameId.Value, s);
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
#else

#endif // WINDOWS_UWP


}
