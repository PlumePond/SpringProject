
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
                    levelObject.color, 
                    levelObject.transform.position, 
                    levelObject.size, 
                    levelObject.transform.rotation,
                    levelObject.transform.flipX,
                    levelObject.transform.flipY,
                    i, 
                    $"{levelObject.data.folder}/{levelObject.data.name}"
                );
                levelObjects.Add(data);
            }
        }

        GridSaveData saveData = new GridSaveData(grid.BackgroundColor, grid.FogColor, levelObjects.ToArray());

        string path = Path.Combine(SAVE_PATH, $"{ActiveLevel}.json");
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static void LoadAll()
    {
        string path = SAVE_PATH;

        foreach (string jsonFile in Directory.GetFiles(path, "*.json"))
        {
            string json = File.ReadAllText(jsonFile);
            string name = Path.GetFileNameWithoutExtension(jsonFile);

            GridSaveData saveData = JsonConvert.DeserializeObject<GridSaveData>(json);
            LoadedLevelsData.Add(name, saveData);
        }
    }

    public static void OpenLevel(string level, Grid grid)
    {
        if (!LoadedLevelsData.ContainsKey(level))
        {
            throw new System.Exception($"Level '{level}' is not loaded. Cannot open level.");
        }

        GridSaveData saveData = LoadedLevelsData[level];

        if (saveData != null)
        {
            grid.LoadLevelObjects(saveData.levelObjects);
            grid.SetFogColor(saveData.fogColor);
            grid.SetBackgroundColor(saveData.backgroundColor); 

            Debug.Log($"Level '{level}' loaded successfully.");
        }
        else
        {
            Debug.Log($"Level '{level}' failed to load.");
        }

        ActiveLevel = level;
    }

    public static void Load(string level, Grid grid)
    {
        string path = Path.Combine(SAVE_PATH, $"{level}.json");
        string json = File.ReadAllText(path);

        GridSaveData saveData = JsonConvert.DeserializeObject<GridSaveData>(json);

        if (saveData != null)
        {
            grid.LoadLevelObjects(saveData.levelObjects);
            grid.SetFogColor(saveData.fogColor);
            grid.SetBackgroundColor(saveData.backgroundColor); 

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
    public GridSaveData(Color backgroundColor, Color fogColor, LevelObjectSaveData[] levelObjects)
    {
        this.backgroundColor = backgroundColor;
        this.fogColor = fogColor;
        this.levelObjects = levelObjects;
    }

    public Color backgroundColor { get; set; }
    public Color fogColor { get; set; }
    public LevelObjectSaveData[] levelObjects { get; set; }
}

public class LevelObjectSaveData
{
    public LevelObjectSaveData(Color color, Point position, Point size, int rotation, bool flipX, bool flipY, int layer, string dataKey)
    {
        this.color = color;
        this.position = position;
        this.size = size;
        this.rotation = rotation;
        this.flipX = flipX;
        this.flipY = flipY;
        this.layer = layer;
        this.dataKey = dataKey;
    }

    public Color color { get; set; }
    public Point position { get; set; }
    public Point size { get; set; }
    public int rotation { get; set; }
    public bool flipX { get; set; }
    public bool flipY { get; set; }
    public int layer { get; set; }
    public string dataKey { get; set; }
}