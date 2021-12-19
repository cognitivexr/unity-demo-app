namespace CognitiveXR.CogStream
{
    public struct Message
    {
        public int type;
        public Content content;
    }

    public struct Content
    {
        public string code;
        public string engine;
        public Attributes attributes;
    }
}