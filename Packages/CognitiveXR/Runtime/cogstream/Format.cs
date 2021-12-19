namespace CognitiveXR.CogStream
{
    public enum ColorMode
    {
        UNKNOWN = 0,
        RGB = 1,
        RGBA = 2,
        Gray = 3,
        BGR = 4,
        BGRA = 5,
        HLS = 6,
        Lab = 7,
        Luv = 8,
        Bayer = 9
    }

    public enum Orientation
    {
        UNKNOWN = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3,
        BottomLeft = 4,
        LeftTop = 5,
        RightTop = 6,
        RightBottom = 7,
        LeftBottom = 8
    }

    [System.Serializable]
    public struct Format
    {
        public int width;
        public int height;
        public ColorMode colorMode;
        public Orientation orientation;
    }
}
