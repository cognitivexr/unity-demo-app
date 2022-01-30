using System.Collections.Generic;
using CognitiveXR.CogStream;

[System.Serializable]
public class YOLOEngineResult : EngineResult
{
    public List<float> xyxy;
    public float conf;
    public string label;
}