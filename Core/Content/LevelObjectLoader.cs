using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Content;

public static class LevelObjectLoader
{
    static Dictionary<string, LevelObjectData> _levelObjectDataDictionary = new Dictionary<string, LevelObjectData>();
    public static Dictionary<string, LevelObjectData> LevelObjectDataDictionary => _levelObjectDataDictionary;

    // load level objects from json and png files in the content directory
    public static void Load(string contentRoot, GraphicsDevice graphicsDevice)
    {
        // check if content directory exists
        if (!Directory.Exists(contentRoot))
        {
            Debug.Log($"Content root not found: {contentRoot}");
        }

        // iterate through each category folder
        foreach (string folder in Directory.GetDirectories(contentRoot))
        {
            string folderName = Path.GetFileName(folder);

            // iterate through each json file in the folder
            foreach (string jsonFile in Directory.GetFiles(folder, "*.json"))
            {
                string objectName = Path.GetFileNameWithoutExtension(jsonFile);
                string texture = Path.Combine(folder, objectName + ".png");
                string outlineTexture = Path.Combine(folder, objectName + "_outline.png");

                try
                {
                    // parse json
                    string json = File.ReadAllText(jsonFile);
                    Debug.Log("Level Object .json file found: " + jsonFile);

                    LevelObjectJsonData data = JsonSerializer.Deserialize<LevelObjectJsonData>(json);

                    // load matching png
                    Texture2D sprite = null;
                    if (File.Exists(texture))
                    {
                        using FileStream stream = File.OpenRead(texture);
                        sprite = Texture2D.FromStream(graphicsDevice, stream);
                    }
                    else
                    {
                        Debug.Log($"Warning: No matching texture for '{objectName}' in '{folder}'");
                    }

                    // load outline texture if it exists
                    Texture2D outline = null;
                    if (File.Exists(outlineTexture))
                    {
                        using FileStream stream = File.OpenRead(outlineTexture);
                        outline = Texture2D.FromStream(graphicsDevice, stream);
                    }
                    else
                    {
                        Debug.Log($"Warning: No matching outline texture for '{objectName}' in '{folder}'");
                    }

                    Material material = Material.Default;

                    if (Enum.TryParse<Material>(data.material, true, out material))
                    {
                        Debug.Log("Successfully parsed Material");
                    }
                    else
                    {
                        Debug.Log("Failed to parse Material");
                    }

                    // instantiate new levelObject
                    var levelObjectData = new LevelObjectData(objectName, folderName, material, sprite, outline, data.solid);

                    // assign value to dictionary
                    string key = $"{folderName}/{objectName}";
                    _levelObjectDataDictionary[key] = levelObjectData;

                    Debug.Log($"Loaded: {key}");
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to load '{jsonFile}: '{ex.Message}");
                }
            }
        }
    }
}