using System;
using System.Collections;
using System.Collections.Generic;
using CognitiveXR.Cpop;
using UnityEngine;

public class CognitiveXRManager : MonoBehaviour
{
    private CpopSubscriber cpopSubscriber;

    public string CpopServerAddress = "localhost";

    private void Awake()
    {
        cpopSubscriber = new CpopSubscriber(new CpopServerOptions{ Server = CpopServerAddress});
        cpopSubscriber.Subscribe();
    }

    private void OnDestroy()
    {
        if ((cpopSubscriber is null))
        {
            cpopSubscriber.Unsubscribe();
        }
    }

    private void Update()
    {
        Debug.Assert(cpopSubscriber != null);

        if (cpopSubscriber.Queue.TryDequeue(out CpopData cpopData))
        {
            Debug.LogError("CPOP DATA DEQUEUED");
            EventManager.Instance.FireEvent_BBUpdate(cpopData);
        }
    }
}
