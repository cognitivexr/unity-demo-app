using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class YOLODetector : MonoBehaviour
{
    [SerializeField] private GameObject BoundingBoxPrefab;
    [SerializeField] private HLImageSenderComponent imageSenderComponent;
    private readonly List<BoundingBox> spawnedBoxes = new List<BoundingBox>();

    private ConcurrentQueue<BoundingBox.BoundingBoxInfo> receivedBoundingBoxEvents = new ConcurrentQueue<BoundingBox.BoundingBoxInfo>();
    
    private void Start()
    {
        imageSenderComponent.SetReceiveChannel(new YOLOReceiveChannel());
        Task.Run(UpdateYoloEngineResults);
    }

    private async void UpdateYoloEngineResults()
    {
        await new WaitUntil(() => imageSenderComponent.GetEngine() != null);

        YOLOReceiveChannel receiveChannel = imageSenderComponent.GetEngine().GetReceiveChannel<YOLOReceiveChannel>();
        if(receiveChannel == null) return;

        while (true)
        {
            List<YOLOEngineResult> yoloEngineResults = await receiveChannel.Next<YOLOEngineResult>();
            imageSenderComponent.SetReceivedNewFrame(true);

            foreach (YOLOEngineResult engineResult in yoloEngineResults)
            {
                HLImageSenderComponent.SampleStruct s = imageSenderComponent.GetSample();

                Vector3 pos1 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                    s.resolution,
                    new Vector2(engineResult.xyxy[0], engineResult.xyxy[1]));
                Vector3 pos2 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                    s.resolution,
                    new Vector2(engineResult.xyxy[0], engineResult.xyxy[1] + engineResult.xyxy[3]));
                Vector3 pos4 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                    s.resolution,
                    new Vector2(engineResult.xyxy[0] + engineResult.xyxy[2], engineResult.xyxy[1]));
                Vector3 pos3 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                    s.resolution,
                    new Vector2(engineResult.xyxy[0] + engineResult.xyxy[2],
                        engineResult.xyxy[1] + engineResult.xyxy[3]));
                
                receivedBoundingBoxEvents.Enqueue(new BoundingBox.BoundingBoxInfo()
                {
                    Bounds = new List<Vector3>()
                    {
                        pos1, pos2, pos3, pos4
                    },
                    text = engineResult.label,
                    frameId = engineResult.frameId,
                    cameraPose = s.cameraPose
                });

            }
        }
    }
    
    private void Update()
    {
        while (receivedBoundingBoxEvents.TryDequeue(out BoundingBox.BoundingBoxInfo info))
        {
            CleanupOldEmotionBoxes(info.frameId);

            GameObject boundingBoxGO = Instantiate(BoundingBoxPrefab, info.cameraPose.position, Quaternion.identity);

            BoundingBox boundingBox = boundingBoxGO.GetComponent<BoundingBox>();

            boundingBox.Set(info);
        
            spawnedBoxes.Add(boundingBox);
        }
    }

    private void CleanupOldEmotionBoxes(uint newFrameId)
    {
        foreach (BoundingBox spawnedBox in spawnedBoxes)
        {
            if(spawnedBox == null) continue;
            
            if (spawnedBox.Info.frameId < newFrameId)
            {
                Destroy(spawnedBox.gameObject);
            }
        }
    }
}
