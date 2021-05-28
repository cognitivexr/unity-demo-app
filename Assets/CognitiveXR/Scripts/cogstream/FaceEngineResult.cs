using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class FaceEngineResult : EngineResult
{
    public List<int> face;
    public List<Emotions> emotions;
}

[System.Serializable]
public struct Emotions
{
    public float probability;
    public string label;
}

[System.Serializable]
public struct FaceEngineResultData
{
    public List<int> face;
    public List<Emotions> emotions;
}