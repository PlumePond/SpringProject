using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpringProject.Core.Content;

public static class TextureLoader
{
    public static Dictionary<string, Texture2D> Load(string folder, GraphicsDevice graphicsDevice)
    {
        var dictionary = new Dictionary<string, Texture2D>();

        foreach (string pngFile in Directory.GetFiles(folder, "*.png"))
        {
            string name = Path.GetFileNameWithoutExtension(pngFile);
            Debug.Log("Texture .png file found: " + pngFile);

            using FileStream stream = File.OpenRead(pngFile);
            Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);

            dictionary[name] = texture;
        }

        return dictionary;
    }
}