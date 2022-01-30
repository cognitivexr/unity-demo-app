using System.Collections.Generic;
using CognitiveXR.CogStream;
using CognitiveXR.SimpleJSON;

public class YOLOReceiveChannel : ResultReceiveChannel
{
    protected override List<EngineResult> ParseResultPacket(ResultPacket resultPacket)
    {
        string jsonText = System.Text.Encoding.UTF8.GetString(resultPacket.data);
        if(string.IsNullOrEmpty(jsonText) || jsonText.Length <= 2) return new List<EngineResult>(); 

        JSONNode json = JSON.Parse(jsonText);
        int yoloCount = json.AsArray.Count;
        var arr = json.AsArray;

        List<EngineResult> results = new List<EngineResult>();

        for (int i = 0; i < yoloCount; ++i)
        {
            var element = arr[i];

            YOLOEngineResult engineResult = new YOLOEngineResult()
            {
                frameId = resultPacket.frameId,
                seconds = resultPacket.seconds,
                nanoseconds = resultPacket.nanoseconds,
                result = System.Text.Encoding.UTF8.GetString(resultPacket.data),
                // todo bounds
                conf = element["conf"],
                label = element["label"]
            };
            
            results.Add(engineResult);
        }

        return results;
    }
}
