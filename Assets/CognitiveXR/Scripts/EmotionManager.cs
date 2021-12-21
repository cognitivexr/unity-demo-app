using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EmotionManager : MonoBehaviour
{
    [SerializeField] private GameObject EmotionBoxPrefab;
    [SerializeField] private HLImageSenderComponent emotionDetectionComponent;

    private readonly List<EmotionBox> spawnedBoxes = new List<EmotionBox>();

    private ConcurrentQueue<EmotionBox.EmotionInfo> receivedEmotionDetectedEvents = new ConcurrentQueue<EmotionBox.EmotionInfo>();

    private void Awake()
    {
        Debug.Assert(EmotionBoxPrefab != null);
        emotionDetectionComponent.OnEmotionDetected += OnEmotionDetected;
    }

    private void Update()
    {
        while (receivedEmotionDetectedEvents.TryDequeue(out EmotionBox.EmotionInfo info))
        {
            CleanupOldEmotionBoxes(info.frameId);

            GameObject emotionBoxGO = Instantiate(EmotionBoxPrefab, info.cameraPose.position, Quaternion.identity);// info.cameraPose.rotation );

            EmotionBox emotionBox = emotionBoxGO.GetComponent<EmotionBox>();

            emotionBox.Init(info);
        
            spawnedBoxes.Add(emotionBox);
        }
    }

    private void OnEmotionDetected(EmotionBox.EmotionInfo info)
    {
        receivedEmotionDetectedEvents.Enqueue(info);
    }

    private void CleanupOldEmotionBoxes(uint newFrameId)
    {
        foreach (EmotionBox spawnedBox in spawnedBoxes)
        {
            if(spawnedBox == null) continue;
            
            if (spawnedBox.Info.frameId < newFrameId)
            {
                Destroy(spawnedBox.gameObject);
            }
        }
    }
}
