using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;


public class FaceReceiveChannel : ResultReceiveChannel
{
    public override void Receive(ResultPacket resultPacket)
    {
        string json = System.Text.Encoding.UTF8.GetString(resultPacket.data);
        FaceEngineResultData resultData = JsonUtility.FromJson<FaceEngineResultData>(json);

        FaceEngineResult engineResult = new FaceEngineResult()
        {
            frameId = resultPacket.frameId,
            seconds = resultPacket.seconds,
            nanoseconds = resultPacket.nanoseconds,
            result = System.Text.Encoding.UTF8.GetString(resultPacket.data),
            emotions = resultData.emotions,
            face = resultData.face
        };

        engineResultQueue.Enqueue(engineResult);
    }
}
