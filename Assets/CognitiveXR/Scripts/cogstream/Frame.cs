using System;

public struct Frame
{
    public uint frameId;
    public DateTime timestamp;
    public int width;
    public int height;
    public byte[] rawFrame;
}