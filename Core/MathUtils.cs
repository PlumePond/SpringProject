namespace SpringProject.Core;

public static class MathUtils
{
    public static float Lerp(float from, float to, float value)
    {
        return (1 - value) * from + value * to;
    }

    public static float InverseLerp(float from, float to, float value)
    {
        return (value - from) / (to - from);
    }
}