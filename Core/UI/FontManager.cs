using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System;
using FontStashSharp;

namespace SpringProject.Core.UI;

public class FontManager
{
    public static Dictionary<string, SpriteFontBase> Fonts { get; private set; } = new Dictionary<string, SpriteFontBase>();

    public static SpriteFontBase Get(string name)
    {
        if (Fonts.TryGetValue(name, out SpriteFontBase font))
        {
            return font;
        }
        else
        {
            Console.WriteLine($"Font '{name}' not found.");
            return null;
        }
    }

    public static void SetFonts(Dictionary<string, SpriteFontBase> fonts)
    {
        Fonts = fonts;
    }
}