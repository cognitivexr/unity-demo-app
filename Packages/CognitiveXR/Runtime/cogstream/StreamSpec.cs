using System;
using System.Collections.Generic;

namespace CognitiveXR.CogStream
{
    [System.Serializable]
    public class Attributes : Dictionary<string, List<string>>
    {
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

    [System.Serializable]
    public struct Specification
    {
        public string name;
        //public Format input_format;
        public Attributes attributes;
    }

    [System.Serializable]
    public struct EngineDescriptor
    {
        public string name;
        public Specification specification;
    }

    [System.Serializable]
    public struct StreamMetadata
    {
        public StreamSpec spec;
        public Format clientFormat;
        public Format engineFormat;
    }
}