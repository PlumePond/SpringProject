using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpringProject.Core.Content;

public class EditorObjectLoader
{
    public Dictionary<string, LevelObjectData> LevelObjectsDatas => _levelObjectsDatas;

    Dictionary<string, LevelObjectData> _levelObjectsDatas = new Dictionary<string, LevelObjectData>();
    GraphicsDevice _graphicsDevice;
    string _contentRoot;

    public EditorObjectLoader(GraphicsDevice graphicsDevice, string contentRoot)
    {
        _graphicsDevice = graphicsDevice;
        _contentRoot = contentRoot;
    }

    // load level objects from json and png files in the content directory
    public void LoadLevelObjects()
    {
        // check if content directory exists
        if (!Directory.Exists(_contentRoot))
        {
            Debug.WriteLine($"Content root not found: {_contentRoot}");
        }

        // iterate through each category folder
        foreach (string folder in Directory.GetDirectories(_contentRoot))
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
                    string jsonText = File.ReadAllText(jsonFile);
                    LevelObjectJsonData jsonData = JsonSerializer.Deserialize<LevelObjectJsonData>(jsonText);

                    // load matching png
                    Texture2D sprite = null;
                    if (File.Exists(texture))
                    {
                        using FileStream stream = File.OpenRead(texture);
                        sprite = Texture2D.FromStream(_graphicsDevice, stream);
                    }
                    else
                    {
                        Debug.WriteLine($"Warning: No matching texture for '{objectName}' in '{folder}'");
                    }

                    // load outline texture if it exists
                    Texture2D outline = null;
                    if (File.Exists(outlineTexture))
                    {
                        using FileStream stream = File.OpenRead(outlineTexture);
                        outline = Texture2D.FromStream(_graphicsDevice, stream);
                    }
                    else
                    {
                        Debug.WriteLine($"Warning: No matching outline texture for '{objectName}' in '{folder}'");
                    }

                    // parse material enum
                    if (!Enum.TryParse<Material>(jsonData.material, true, out Material material))
                    {
                        Debug.WriteLine($"Warning: Unknown material '{jsonData.material}' in '{jsonFile}'");
                        material = Material.Default;
                    }

                    // instantiate new levelObject
                    var levelObjectData = new LevelObjectData(objectName, folderName, material, sprite, outline, jsonData.solid);

                    // assign value to dictionary
                    string key = $"{folderName}/{objectName}";
                    _levelObjectsDatas[key] = levelObjectData;

                    Debug.WriteLine($"Loaded: {key}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load '{jsonFile}: '{ex.Message}");
                }
            }
        }
    }
}