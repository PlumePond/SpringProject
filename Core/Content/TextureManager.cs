using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace SpringProject.Core.Content;

public class TextureManager
{
    public static Dictionary<string, Texture2D> Textures { get; private set; } = new Dictionary<string, Texture2D>();

    public static Texture2D Get(string name)
    {
        if (Textures.TryGetValue(name, out Texture2D texture))
        {
            return texture;
        }
        else
        {
            Console.WriteLine($"Texture '{name}' not found.");
            return null;
        }
    }

    public static void SetTextures(Dictionary<string, Texture2D> textures)
    {
        Textures = textures;
    }
}