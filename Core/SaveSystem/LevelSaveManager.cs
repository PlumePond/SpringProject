
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using Newtonsoft.Json;
using SpringProject.Core.Scenes;
using MemoryPack;

namespace SpringProject.Core.SaveSystem;

public static class LevelSaveManager
{
    const string SAVE_PATH = "Assets/Levels";

    public static string ActiveLevel = "";

    public static List<string> DiscoveredLevels = new List<string>();

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
                        data.Parameters[param.Label] = JsonConvert.SerializeObject(param.GetValue());
                    }
                }

                levelObjects.Add(data);
            }
        }

        ColorSaveData colorSaveData = new ColorSaveData(ColorManager.Colors.ToArray());

        GridSaveData saveData = new GridSaveData(grid.BackgroundColorIndex, grid.FogColorIndex, grid.size, levelObjects.ToArray(), colorSaveData);

        string path = Path.Combine(SAVE_PATH, $"{ActiveLevel}.level");
        // string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        var binary = MemoryPackSerializer.Serialize(saveData);
        File.WriteAllBytes(path, binary);
    }

    public static void LoadAll()
    {
        string path = SAVE_PATH;
        
        DiscoveredLevels.Clear();

        ConvertAllJsonLevelDataToBinaryFormat();

        foreach (string saveFile in Directory.GetFiles(path, "*.level"))
        {
            //byte[] data = File.ReadAllBytes(saveFile);
            string name = Path.GetFileNameWithoutExtension(saveFile);

            //GridSaveData saveData = MemoryPackSerializer.Deserialize<GridSaveData>(data);
            DiscoveredLevels.Add(name);
        }
    }

    public static void ConvertAllJsonLevelDataToBinaryFormat()
    {
        string path = SAVE_PATH;

        foreach (string jsonFile in Directory.GetFiles(path, "*.json"))
        {
            string json = File.ReadAllText(jsonFile);
            string name = Path.GetFileNameWithoutExtension(jsonFile);
            GridSaveData saveData = JsonConvert.DeserializeObject<GridSaveData>(json);
            
            string outputPath = Path.Combine(SAVE_PATH, $"{name}.level");
            var binary = MemoryPackSerializer.Serialize(saveData);
            File.WriteAllBytes(outputPath, binary);
        }
    }

    public static void Load(string level, Grid grid)
    {
        string path = Path.Combine(SAVE_PATH, $"{level}.level");
        byte[] data = File.ReadAllBytes(path);

        if (data.Length > 0)
        {
            GridSaveData saveData = MemoryPackSerializer.Deserialize<GridSaveData>(data);

            if (saveData != null)
            {
                grid.SetSize(saveData.Size);
                grid.LoadLevelObjects(saveData.LevelObjects);
                grid.SetFogColorIndex(saveData.FogColorIndex);
                grid.SetBackgroundColorIndex(saveData.BackgroundColorIndex);
                ColorManager.Load(saveData.Color?.Colors);
                Debug.Log($"Level '{level}' loaded successfully.");
            }
            else
            {
                Debug.Log($"Level '{level}' failed to load.");
            }
        }
        else
        {
            Debug.Log($"Level '{level}' is empty, starting fresh.");
        }

        ActiveLevel = level;
    }
}

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class GridSaveData
{
    public GridSaveData(int backgroundColorIndex, int fogColorIndex, Point size, LevelObjectSaveData[] levelObjects, ColorSaveData color)
    {
        BackgroundColorIndex = backgroundColorIndex;
        FogColorIndex = fogColorIndex;
        Size = size;
        LevelObjects = levelObjects;
        Color = color;
    }
    
    [MemoryPackOrder(0)] public int BackgroundColorIndex { get; set; }
    [MemoryPackOrder(1)] public int FogColorIndex { get; set; }
    [MemoryPackOrder(2)] public Point Size { get; set; }
    [MemoryPackOrder(3)] public LevelObjectSaveData[] LevelObjects { get; set; }
    [MemoryPackOrder(4)] public ColorSaveData Color { get; set; }
}

[MemoryPackable(GenerateType.VersionTolerant)]
public partial class ColorSaveData
{
    [MemoryPackOrder(0)] public Color[] Colors { get; set; }

    public ColorSaveData(Color[] colors)
    {
        Colors = colors;
    }
}


[MemoryPackable(GenerateType.VersionTolerant)]
public partial class LevelObjectSaveData
{
    [MemoryPackOrder(0)] public int ColorIndex { get; set; }
    [MemoryPackOrder(1)] public Point Position { get; set; }
    [MemoryPackOrder(2)] public Point Size { get; set; }
    [MemoryPackOrder(3)] public int Rotation { get; set; }
    [MemoryPackOrder(4)] public bool FlipX { get; set; }
    [MemoryPackOrder(5)] public bool FlipY { get; set; }
    [MemoryPackOrder(6)] public int Layer { get; set; }
    [MemoryPackOrder(7)] public string DataKey { get; set; }
    [MemoryPackOrder(8)] [JsonConverter(typeof(StringifyValuesConverter))] public Dictionary<string, string> Parameters { get; set; } = new(); // the string key will be converted from json

    public LevelObjectSaveData(int colorIndex, Point position, Point size, int rotation, bool flipX, bool flipY, int layer, string dataKey)
    {
        ColorIndex = colorIndex;
        Position = position;
        Size = size;
        Rotation = rotation;
        FlipX = flipX;
        FlipY = flipY;
        Layer = layer;
        DataKey = dataKey;
    }
}