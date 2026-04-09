using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using SpringProject.Core.Debugging;
using SpringProject.Core.Content.Types;
using System.Drawing;

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

                try
                {
                    // parse json
                    string json = File.ReadAllText(jsonFile);
                    LevelObjectJsonData data = JsonConvert.DeserializeObject<LevelObjectJsonData>(json);

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

                    Material material = Material.Default;
                    Enum.TryParse(data.material, true, out material);
                    Type type = typeof(LevelObject);

                    if (LevelObjectTypeLoader.Types.ContainsKey(data.type))
                    {
                        type = LevelObjectTypeLoader.Types[data.type];
                    }

                    // instantiate new levelObject
                    var levelObjectData = new LevelObjectData(objectName, folderName, material, sprite, data.solid, type, data.scalable, data.frame, data.hitbox, data.defaultFramePos, data.frameOutline, data.enforceGrid, data.tags, data.placeSound);
                    levelObjectData.path = Path.Combine(contentRoot, folderName, objectName);

                    // assign value to dictionary
                    string key = $"{folderName}/{objectName}";
                    _levelObjectDataDictionary[key] = levelObjectData;
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to load '{jsonFile}: '{ex.Message}");
                }
            }
        }

        Debug.Log($"Level Objects loaded! ({_levelObjectDataDictionary.Count})");
    }

    public static LevelObjectData Get(string key)
    {
        return LevelObjectDataDictionary[key];
    }
}