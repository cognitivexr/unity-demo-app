using System;
using CognitiveXR.Cpop;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public delegate void PublishBBUpdateEvent(CpopData updateData);
    public static event PublishBBUpdateEvent PublishBBUPdate;

    public void FireEvent_BBUpdate(CpopData _updateData)
    {
        PublishBBUPdate?.Invoke(_updateData);
    }
    
    private void Awake()
    {
        Instance = this;
    }

}
