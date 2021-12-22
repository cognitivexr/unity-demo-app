using System.Collections.Generic;
using SimpleJSON;
using CognitiveXR.CogStream;

public class FaceReceiveChannel : ResultReceiveChannel
{
    protected override List<EngineResult> ParseResultPacket(ResultPacket resultPacket)
    {
        string jsonText = System.Text.Encoding.UTF8.GetString(resultPacket.data);
        if(string.IsNullOrEmpty(jsonText) || jsonText.Length <= 2) return new List<EngineResult>(); // TODO: error handling

        JSONNode json = JSON.Parse(jsonText);
        int facesNumber = json.AsArray.Count;
        var arr = json.AsArray;

        List<EngineResult> results = new List<EngineResult>();

        for (int i = 0; i < facesNumber; ++i)
        {
            var element = arr[i];
            var faces = element["face"];
            var emotions = element["emotions"];

            FaceEngineResult engineResult = new FaceEngineResult()
            {
                frameId = resultPacket.frameId,
                seconds = resultPacket.seconds,
                nanoseconds = resultPacket.nanoseconds,
                result = System.Text.Encoding.UTF8.GetString(resultPacket.data),
                emotions = EmotionsFromJson(emotions),
                face = FaceFromJson(faces)
            };

            results.Add(engineResult);
        }

        return results;
    }

    private List<int> FaceFromJson(JSONNode jsonNode)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < jsonNode.Count; ++i)
        {
            result.Add(jsonNode[i].AsInt);
        }
        
        return result;
    }

    private List<Emotion> EmotionsFromJson(JSONNode jsonNode)
    {
        List<Emotion> emotions = new List<Emotion>();

        for (int i = 0; i < jsonNode.Count; ++i)
        {
            Emotion emotion = new Emotion
            {
                label = jsonNode[i]["label"].ToString(),
                probability = jsonNode[i]["probability"].AsFloat
            };
            emotions.Add(emotion);
        }
        
        return emotions;
    }
}
