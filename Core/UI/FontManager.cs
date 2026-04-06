using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System;
using FontStashSharp;
using SpringProject.Core.Content;

namespace SpringProject.Core.UI;

public class FontManager
{
    public static Dictionary<string, Font> Fonts { get; private set; } = new Dictionary<string, Font>();

    public static Font Get(string name)
    {
        if (Fonts.TryGetValue(name, out Font font))
        {
            return font;
        }
        else
        {
            Console.WriteLine($"Font '{name}' not found.");
            return null;
        }
    }

    public static void SetFonts(Dictionary<string, Font> fonts)
    {
        Fonts = fonts;
    }
}