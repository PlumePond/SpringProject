using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpringProject.Core.Debugging;

public static class Debug
{
    static Texture2D _pixel;

    // initialize debug and create 1x1 pixel texture
    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    // writes a message to the console
    public static void Log(string message)
    {
        Console.WriteLine(message);
    }

    public static void Fail(string message)
    {
        System.Diagnostics.Debug.Fail(message);
    }

    // draws a rectangle outline for debugging purposes
    public static void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness = 1)
    {
        // top
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, rect.Width, thickness), color);
        // right
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), color);
        // bottom
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);
        // left
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, thickness, rect.Height), color);
    }

    // draws a rectangle for debugging purposes
    public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        spriteBatch.Draw(_pixel, rect, color);
    }

    // draws a circle for debugging purposes
    public static void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, float thickness = 1.0f, int segments = 16)
    {
        float increment = MathF.PI * 2.0f / segments;
        float theta = 0.0f;

        for (int i = 0; i < segments; i++)
        {
            Vector2 start = center + radius * new Vector2(MathF.Cos(theta), MathF.Sin(theta));
            Vector2 end = center + radius * new Vector2(MathF.Cos(theta + increment), MathF.Sin(theta + increment));

            DrawLine(spriteBatch, start, end, color, thickness);

            theta += increment;
        }
    }

    public static void DrawCircle(SpriteBatch spriteBatch, Point center, float radius, Color color, float thickness = 1.0f, int segments = 16)
    {
        DrawCircle(spriteBatch, center.ToVector2(), radius, color, thickness, segments);
    }

    // draws a line for debugging purposes
    public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 1.0f)
    {
        Vector2 edge = end - start;

        // calculate angle to rotate line
        float angle = (float)Math.Atan2(edge.Y, edge.X);

        Vector2 stretch = new Vector2(edge.Length(), thickness);

        //spriteBatch.Draw(_pixel, rect, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);

        spriteBatch.Draw(_pixel, start, null, color, angle, Vector2.Zero, stretch, SpriteEffects.None, 0f);
    }

    public static void DrawLine(SpriteBatch spriteBatch, Point start, Point end, Color color, float thickness = 1.0f)
    {
        DrawLine(spriteBatch, start.ToVector2(), end.ToVector2(), color, thickness);
    }
}