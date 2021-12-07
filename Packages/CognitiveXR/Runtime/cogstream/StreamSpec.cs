using System.Collections.Generic;

namespace CognitiveXR.CogStream
{
    [System.Serializable]
    public class Attributes : Dictionary<string, List<string>>
    {
        public Attributes()
        {
        }

        public Attributes Set(string key, string value)
        {
            List<string> values = new List<string>() {value};
            Add(key, values);
            return this;
        }
    }

    [System.Serializable]
    public struct StreamSpec
    {
        public string engineAddress;
        public Attributes attributes;
    }
}