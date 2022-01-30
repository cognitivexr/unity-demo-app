using System.Collections.Generic;
using CognitiveXR.CogStream;

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
