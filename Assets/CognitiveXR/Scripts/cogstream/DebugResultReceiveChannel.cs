using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DebugResult
{
    public int frame_id;
    public int answer;
}

public class DebugResultReceiveChannel : IResultReceiveChannel<DebugResult>
{


    public EngineResult<DebugResult> Receive()
    {
        reader.Get()

        return null;
    }
}
