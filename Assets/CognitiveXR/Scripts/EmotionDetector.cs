using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class EmotionDetector : MonoBehaviour
{
    [SerializeField] private GameObject BoundingBoxPrefab;
    [SerializeField] private HLImageSenderComponent imageSenderComponent;
    private readonly List<BoundingBox> spawnedBoxes = new List<BoundingBox>();

    private ConcurrentQueue<BoundingBox.BoundingBoxInfo> receivedEmotionDetectedEvents = new ConcurrentQueue<BoundingBox.BoundingBoxInfo>();

    private void Start()
    {
        imageSenderComponent.SetReceiveChannel(new FaceReceiveChannel());
        Task.Run(UpdateFaceEngineResults);
    }

    private async void UpdateFaceEngineResults()
    {
        // wait until the engine is alive
        await new WaitUntil(() => imageSenderComponent.GetEngine() != null);

        FaceReceiveChannel receiveChannel = imageSenderComponent.GetEngine().GetReceiveChannel<FaceReceiveChannel>();
        if (receiveChannel == null) return;

        while (true)
        {
            List<FaceEngineResult> faceEngineResults = await receiveChannel.Next<FaceEngineResult>();
            imageSenderComponent.SetReceivedNewFrame(true);

            foreach (FaceEngineResult engineResult in faceEngineResults)
            {
                if (engineResult.emotions.Count > 0)
                {

                    HLImageSenderComponent.SampleStruct s = imageSenderComponent.GetSample();

                    Vector3 pos1 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                        s.resolution,
                        new Vector2(engineResult.face[0], engineResult.face[1]));
                    Vector3 pos2 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                        s.resolution,
                        new Vector2(engineResult.face[0], engineResult.face[1] + engineResult.face[3]));
                    Vector3 pos4 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                        s.resolution,
                        new Vector2(engineResult.face[0] + engineResult.face[2], engineResult.face[1]));
                    Vector3 pos3 = LocatableCameraUtils.PixelCoordToWorldCoord(s.camera2WorldMatrix, s.projectionMatrix,
                        s.resolution,
                        new Vector2(engineResult.face[0] + engineResult.face[2],
                            engineResult.face[1] + engineResult.face[3]));
                    
                    receivedEmotionDetectedEvents.Enqueue(new BoundingBox.BoundingBoxInfo()
                    {
                        Bounds = new List<Vector3>()
                        {
                            pos1, pos2, pos3, pos4
                        },
                        text = engineResult.emotions.Select(x => (x.probability, x)).Max().x.label,
                        frameId = engineResult.frameId,
                        cameraPose = s.cameraPose
                    });
                }
            }
        }
    }
    
    private void Update()
    {
        while (receivedEmotionDetectedEvents.TryDequeue(out BoundingBox.BoundingBoxInfo info))
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
