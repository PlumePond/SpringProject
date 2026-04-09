using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpringProject.Core;

public static class TextureUtils
{
    // generates a white texture based on the input
    // the only color data that remains unchanged is the alpha (hence the name "alpha texture"... pretty cool, right?)
    public static Texture2D GenerateAlphaTexture(Texture2D texture)
    {
        var alphaTexture = new Texture2D(Main.Graphics, texture.Width, texture.Height);
        var pixels = new Color[texture.Width * texture.Height];

        texture.GetData(pixels);

        for (int i = 0; i < pixels.Length; i++)
        {
            // Texture2D.FromStream gives non-premultiplied data,
            // so .A is the true alpha channel
            byte alpha = pixels[i].A;
            pixels[i] = new Color(alpha, alpha, alpha, alpha); // premultiplied white
        }

        alphaTexture.SetData(pixels);
        return alphaTexture;
    }

    // used to draw an expanded outline of an object
    public static void DrawOutlineExpanded(SpriteBatch spriteBatch, Texture2D texture, Rectangle? source, Rectangle destination, Color color)
    {
        var offsets = new Point[]
        {
            new Point(0, -1),
            new Point(1, 0),
            new Point(0, 1),
            new Point(-1, 0)
        };

        foreach (var offset in offsets)
        {
            var offsetDestRect = new Rectangle(destination.Location + offset, destination.Size);
            spriteBatch.Draw(texture, offsetDestRect, source, color);
        }
    }

    public static void DrawOutlineExpanded(SpriteBatch spriteBatch, Texture2D texture, Vector2 pos, Rectangle? source, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float depth)
    {
        var offsets = new Vector2[]
        {
            new Vector2(0, -1),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(-1, 0)
        };

        foreach (var offset in offsets)
        {
            var offsetPos = pos + offset;
            spriteBatch.Draw(texture, offsetPos, source, color, rotation, origin, scale, effects, depth);
        }
    }
}