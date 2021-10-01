using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class FaceEngineResult : EngineResult
{
    public List<int> face;
    public List<Emotion> emotions;
}

[System.Serializable]
public struct Emotion
{
    public float probability;
    public string label;
}
