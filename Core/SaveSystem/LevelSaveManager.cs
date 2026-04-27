
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using Newtonsoft.Json;
using SpringProject.Core.Scenes;

namespace SpringProject.Core.SaveSystem;

public static class LevelSaveManager
{
    const string SAVE_PATH = "Data/Levels";

    public static string ActiveLevel = "";

    public static Dictionary<string, GridSaveData> LoadedLevelsData = new Dictionary<string, GridSaveData>();

    public static void Save(Grid grid)
    {
        Debug.Log("Level saved.");

        List<LevelObjectSaveData> levelObjects = new List<LevelObjectSaveData>();

        for (int i = 0; i < grid.layers.Length; i++)
        {
            foreach (var levelObject in grid.layers[i].LevelObjects)
            {
                LevelObjectSaveData data = new LevelObjectSaveData(
                    levelObject.colorIndex, 
                    levelObject.transform.position, 
                    levelObject.size, 
                    levelObject.transform.rotation,
                    levelObject.transform.flipX,
                    levelObject.transform.flipY,
                    i, 
                    $"{levelObject.data.folder}/{levelObject.data.name}"
                );

                // scan self and components for exposed parameters
                var targets = new List<object> { levelObject };
                targets.AddRange(levelObject.Components);

                foreach (var target in targets)
                {
                    foreach (var param in ParameterScanner.Scan(target))
                    {
                        data.parameters[param.Label] = param.GetValue();
                    }
                }

                levelObjects.Add(data);
            }
        }

        ColorSaveData colorSaveData = new ColorSaveData(ColorManager.Colors.ToArray());

        GridSaveData saveData = new GridSaveData(grid.BackgroundColorIndex, grid.FogColorIndex, grid.size, levelObjects.ToArray(), colorSaveData);

        string path = Path.Combine(SAVE_PATH, $"{ActiveLevel}.json");
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static void LoadAll()
    {
        string path = SAVE_PATH;
        
        LoadedLevelsData.Clear();

        foreach (string jsonFile in Directory.GetFiles(path, "*.json"))
        {
            string json = File.ReadAllText(jsonFile);
            string name = Path.GetFileNameWithoutExtension(jsonFile);

            GridSaveData saveData = JsonConvert.DeserializeObject<GridSaveData>(json);
            LoadedLevelsData.Add(name, saveData);
        }
    }

    public static void Load(string level, Grid grid)
    {
        string path = Path.Combine(SAVE_PATH, $"{level}.json");
        string json = File.ReadAllText(path);

        GridSaveData saveData = JsonConvert.DeserializeObject<GridSaveData>(json);

        if (saveData != null)
        {
            grid.SetSize(saveData.size);
            grid.LoadLevelObjects(saveData.levelObjects);
            grid.SetFogColorIndex(saveData.fogColorIndex);
            grid.SetBackgroundColorIndex(saveData.backgroundColorIndex);

            ColorManager.Load(saveData.color?.colors);

            Debug.Log($"Level '{level}' loaded successfully.");
        }
        else
        {
            Debug.Log($"Level '{level}' failed to load.");
        }

        ActiveLevel = level;
    }
}

public class GridSaveData
{
    public GridSaveData(int backgroundColorIndex, int fogColorIndex, Point size, LevelObjectSaveData[] levelObjects, ColorSaveData color)
    {
        this.backgroundColorIndex = backgroundColorIndex;
        this.fogColorIndex = fogColorIndex;
        this.size = size;
        this.levelObjects = levelObjects;
        this.color = color;
    }
    
    public int backgroundColorIndex { get; set; }
    public int fogColorIndex { get; set; }
    public Point size { get; set; }
    public LevelObjectSaveData[] levelObjects { get; set; }
    public ColorSaveData color { get; set; }
}

public class ColorSaveData(Color[] colors)
{
    public Color[] colors { get; set; } = colors;
}

public class LevelObjectSaveData(int colorIndex, Point position, Point size, int rotation, bool flipX, bool flipY, int layer, string dataKey)
{
    public int colorIndex { get; set; } = colorIndex;
    public Point position { get; set; } = position;
    public Point size { get; set; } = size;
    public int rotation { get; set; } = rotation;
    public bool flipX { get; set; } = flipX;
    public bool flipY { get; set; } = flipY;
    public int layer { get; set; } = layer;
    public string dataKey { get; set; } = dataKey;
    public Dictionary<string, object> parameters { get; set; } = new();
}