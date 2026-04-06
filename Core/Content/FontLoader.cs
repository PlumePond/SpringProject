using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SpringProject.Core.Content;

public static class FontLoader
{
    public static Dictionary<string, Font> Load(string folder)
    {
        var dictionary = new Dictionary<string, Font>();

        // find all files that end with .json in the folder
        foreach(string jsonFile in Directory.GetFiles(folder, "*.json"))
        {
            string name = Path.GetFileNameWithoutExtension(jsonFile);
            string json = File.ReadAllText(jsonFile);

            // use newtonsoft (my love) to deserialize the json
            FontJSONData data = JsonConvert.DeserializeObject<FontJSONData>(json);
            Debug.Log("Font .json file found: " + jsonFile);

            string ttfFilePath = $"{folder}/{name}.ttf";
            FontSystem fontSystem = new FontSystem();

            // check if the .ttf file exists. and if so, scan that shit
            if (File.Exists(ttfFilePath))
            {
                fontSystem.AddFont(File.ReadAllBytes(ttfFilePath));
            }
            else
            {
                Debug.Fail($"Error: .tff file not found for font '{name}'.");
            }

            // I DON'T KNOW WHY DIVIDING IT BY 1.5 WORKS, IT JUST DOES. DON'T QUESTION IT.
            SpriteFontBase fontBase = fontSystem.GetFont(data.Size / 1.5f);
            Font font = new Font(fontBase, data.Size, data.Offset.ToVector2());

            dictionary[name] = font;
        }

        return dictionary;
    }
    
    // font json data (this comment is useless)
    class FontJSONData
    {
        [JsonPropertyName("size")]  public float Size { get; set; } = 16.0f;
        [JsonPropertyName("offset")] public Point Offset { get; set; } = Point.Zero;
    }
}

public class Font(SpriteFontBase fontBase, float size, Vector2 offset)
{
    public SpriteFontBase FontBase = fontBase;
    public float Size = size;
    public Vector2 Offset = offset;
}