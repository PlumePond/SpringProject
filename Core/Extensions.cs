using System;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace SpringProject.Core;

public static class Extensions
{
    public static float Distance(this Point a, Point b)
    {
        float num = a.X - b.X;
        float num2 = a.Y - b.Y;
        return MathF.Sqrt(num * num + num2 * num2);
    }

    public static Vector2[] Vertices(this Rectangle rectangle)
    {
        return
        [
            new Vector2(rectangle.Left, rectangle.Top),
            new Vector2(rectangle.Right, rectangle.Top),
            new Vector2(rectangle.Left, rectangle.Bottom),
            new Vector2(rectangle.Right, rectangle.Bottom)
        ];
    }
}