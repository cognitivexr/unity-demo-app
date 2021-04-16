using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HoloLensCameraStream;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.XR.WSA;
#if WINDOWS_UWP
using global::Windows.Perception.Spatial;
#endif // WINDOWS_UWP

public class Camera2 : MonoBehaviour
{
    private CpopTcpClient cpopTcpClient;
#if ENABLE_WINMD_SUPPORT
    private byte[] latestImageBytes;
    private HoloLensCameraStream.Resolution resolution;

    private Queue<byte[]> imagesToSend;

    private IntPtr spatialCoordinateSystemPtr;
    private HoloLensCameraStream.VideoCapture videoCapture;

    private void Start()
    {
        cpopTcpClient = new CpopTcpClient("192.168.1.104", 4000);

        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureResourceCreatedCallback);
        
#if WINDOWS_UWP
        spatialCoordinateSystemPtr =
 Microsoft.MixedReality.Toolkit.WindowsMixedReality.WindowsMixedRealityUtilities.UtilitiesProvider.ISpatialCoordinateSystemPtr;
#endif // WINDOWS_UWP
        
    }

    private void OnDestroy()
    {
        if (videoCapture != null)
        {
            videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
            videoCapture.Dispose();
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
        
        videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
    }
    
    void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (result.success == false)
        {
            Debug.LogWarning("Could not start video mode.");
            return;
        }

        Debug.Log("Video capture started.");
    }

    void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        if (latestImageBytes == null || latestImageBytes.Length < sample.dataLength)
        {
            latestImageBytes = new byte[sample.dataLength];
        }
        sample.CopyRawImageDataIntoBuffer(latestImageBytes);

        byte[] jpg = ImageConversion.EncodeArrayToJPG(latestImageBytes, GraphicsFormat.B8G8R8A8_SRGB, (uint)resolution.width, (uint)resolution.height, 0U, 50);
        cpopTcpClient.SendImage(jpg);

        //StartCoroutine(nameof(UploadImage), latestImageBytes);
    }
    
    int imageCount = 0;
    
    IEnumerator UploadImage(byte[] data)
    {
        //byte[] imageBuffer = FlipVertical(data.ToList(), resolution.width, resolution.height, 4);
        
        byte[] jpg = ImageConversion.EncodeArrayToJPG(data, GraphicsFormat.B8G8R8A8_SRGB, (uint)resolution.width, (uint)resolution.height, 0U, 50);
        
        WWWForm form = new WWWForm();
        form.AddBinaryData("asset", jpg, $"picture_{resolution.width}_{resolution.height}_{imageCount}.jpg", "images/png");
        imageCount++;
        
        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://192.168.1.104:3000/assets", form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.LogError("failed to send image");
            }
            else
            {
                if (webRequest.isDone)
                {
                    Debug.Log(webRequest.responseCode);
                }
            }

        }
    }

    private byte[] FlipVertical(List<byte> src, int width, int height, int stride)
    {
        byte[] dst = new byte[src.Count];
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                int invY = (height - 1) - y;
                int pxel = (y * width + x) * stride;
                int invPxel = (invY * width + x) * stride;
                for (int i = 0; i < stride; ++i)
                {
                    dst[invPxel + i] = src[pxel + i];
                }
            }
        }

        return dst;
    }

#endif
}
