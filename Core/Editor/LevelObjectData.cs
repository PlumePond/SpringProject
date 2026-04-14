using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.IO;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Editor;

public class LevelObjectData
{
    public string name { get; private set; }
    public string folder { get; private set; }
    public Material material {  get; private set; }
    public Texture2D sprite { get; private set; }
    public Texture2D alphaTexture { get; private set; } = null;
    public bool solid { get; private set; }
    public Type type { get; private set; }
    public bool scalable { get; private set; }
    public Point frame { get; private set; }
    public Rectangle hitbox { get; private set; }
    public string placeSound { get; private set; }
    public Point defaultFramePos { get; private set; }
    public bool frameOutline { get; private set; }
    public bool enforceGrid { get; private set; }
    public string[] tags { get; private set; }

    public string path { get; set; }
    public string typeName { get; set; }

    public Point size => sprite.Bounds.Size;
    public string texturePath => path + ".png";
    public string dataPath => path + ".json";

    public LevelObjectData(string name, string folder, Material material, Texture2D sprite, bool solid, Type type, bool scalable, Point frame, Rectangle hitbox, Point defaultFramePos, bool frameOutline, bool enforceGrid, string[] tags, string placeSound)
    {
        this.name = name;
        this.folder = folder;
        this.material = material;
        this.sprite = sprite;
        this.solid = solid;
        this.type = type;
        this.scalable = scalable;
        this.frame = frame;
        this.defaultFramePos = defaultFramePos;
        this.frameOutline = frameOutline;
        this.enforceGrid = enforceGrid;
        this.tags = tags;
        this.placeSound = placeSound;
        this.hitbox = hitbox;

        alphaTexture = TextureUtils.GenerateAlphaTexture(sprite);

        RuntimeReloader.FileChangedEvent += OnFileChanged;
    }

    void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(e.FullPath)) return; // skip directories
        if (!e.FullPath.Equals(texturePath)) return; // if the path is equal to the texture path

        using (var stream = File.OpenRead(texturePath))
        {
            sprite = Texture2D.FromStream(Main.Graphics, stream);
        }
    }
}

class LevelObjectJsonData
{
    [JsonPropertyName("material")] public string material { get; set; } = "default";
    [JsonPropertyName("solid")] public bool solid { get; set; } = false;
    [JsonPropertyName("type")] public string type { get; set; } = "default";
    [JsonPropertyName("scalable")] public bool scalable { get; set; } = false;
    [JsonPropertyName("frame")] public Point frame { get; set; } = Point.Zero;
    [JsonPropertyName("defaultFramePos")] public Point defaultFramePos { get; set; } = Point.Zero;
    [JsonPropertyName("placeSound")] public string placeSound { get; set; } = null;
    [JsonPropertyName("frameOutline")] public bool frameOutline { get; set; } = false;
    [JsonPropertyName("enforceGrid")] public bool enforceGrid { get; set; } = false;
    [JsonPropertyName("tags")] public string[] tags { get; set; } = null;
    [JsonPropertyName("hitbox")] public Rectangle hitbox { get; set; } = Rectangle.Empty;
}
