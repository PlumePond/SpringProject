using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringProject.Core.Editor;

public class LevelObjectData
{
    public string name { get; private set; }
    public string folder { get; private set; }
    public Material material {  get; private set; }
    public Texture2D sprite { get; private set; }
    public Texture2D outline { get; private set; }
    public bool solid { get; private set; }

    public Point size => sprite.Bounds.Size;

    public LevelObjectData(string name, string folder, Material material, Texture2D sprite, Texture2D outline, bool solid)
    {
        this.name = name;
        this.folder = folder;
        this.material = material;
        this.sprite = sprite;
        this.outline = outline;
        this.solid = solid;
    }
}

public class LevelObjectJsonData
{
    public string material = "default";
    public bool solid = true;
}
