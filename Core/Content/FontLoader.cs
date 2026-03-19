using FontStashSharp;
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

public static class FontLoader
{
    public static Dictionary<string, SpriteFontBase> Load(string folder)
    {
        var dictionary = new Dictionary<string, SpriteFontBase>();

        foreach(string jsonFile in Directory.GetFiles(folder, "*.json"))
        {
            string name = Path.GetFileNameWithoutExtension(jsonFile);
            string json = File.ReadAllText(jsonFile);

            FontJSONData data = JsonSerializer.Deserialize<FontJSONData>(json);
            Debug.Log("Font .json file found: " + jsonFile);

            string ttfFilePath = $"{folder}/{name}.ttf";
            FontSystem fontSystem = new FontSystem();

            if (File.Exists(ttfFilePath))
            {
                fontSystem.AddFont(File.ReadAllBytes(ttfFilePath));
            }
            else
            {
                Debug.Fail($"Error: .tff file not found for font '{name}'.");
            }

            SpriteFontBase font = fontSystem.GetFont(data.Size);

            dictionary[name] = font;
        }

        return dictionary;
    }

    // Private DTOs for deserialization
    class FontJSONData
    {
        [JsonPropertyName("size")]  public int Size { get; set; } = 16;
    }
}