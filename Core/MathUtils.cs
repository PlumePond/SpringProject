using System;
using Microsoft.Xna.Framework;

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

    public static Rectangle LerpRect(Rectangle from, Rectangle to, float value)
    {
        int x = (int)Lerp(from.X, to.X, value);
        int y = (int)Lerp(from.Y, to.Y, value);
        int width = (int)Lerp(from.Width, to.Width, value);
        int height = (int)Lerp(from.Height, to.Height, value);

        return new Rectangle(x, y, width, height);
    }
}