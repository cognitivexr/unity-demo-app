using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Windows.WebCam;

public class HololensFrontCamera : MonoBehaviour
{
    PhotoCapture photoCapture;
    private int imageCount = 0;
    private bool send = true;
    private long startTime;
    
    IEnumerator Start()
    {
        yield return new WaitForSeconds(5.0f);
        // request a photo capture resource without holograms
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    private void OnDestroy()
    {
        photoCapture.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    private void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCapture = captureObject;

        Resolution cameraResolution =
            PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).Last();

        CameraParameters cameraParameters = new CameraParameters
        {
            hologramOpacity = 0.0f,
            cameraResolutionWidth = 640,
            cameraResolutionHeight = 360,
            pixelFormat = CapturePixelFormat.JPEG
        };
        
        photoCapture.StartPhotoModeAsync(cameraParameters, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            StartCoroutine(nameof(StartRecording));
        }
    }

    IEnumerator StartRecording()
    {
        //while (true)
        int count = 0;
        while (count < 10)
        {
            yield return new WaitForSeconds(1.0f);
            yield return new WaitUntil(() => send == true);
            send = false;
            startTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond);
            photoCapture.TakePhotoAsync(OnCapturedPhotoToMemory);
            count++;
        }
    }

    private long duration;
    private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            duration = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - startTime;
            List<byte> imageBuffer = new List<byte>();
            
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBuffer);

            StartCoroutine(nameof(UploadImage), imageBuffer.ToArray());
        }
    }

    IEnumerator UploadImage(byte[] data)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("asset", data, $"picture_{imageCount}_{duration}.jpg", "images/png");
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
                    send = true;
                }
            }

        }
    }
    
    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCapture.Dispose();
        photoCapture = null;
    }
}
