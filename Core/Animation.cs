namespace SpringProject.Core;

public struct Animation
{
    public int Index { get; set; }
    public int FrameCount { get; set; }
    public float FrameInterval { get; set; }
    public bool Loop { get; set; }

    public float Length => FrameCount * FrameInterval;

    public Animation(int index, int frameCount, float frameInterval, bool loop)
    {
        Index = index;
        FrameCount = frameCount;
        FrameInterval = frameInterval;
        Loop = loop;
    }
}