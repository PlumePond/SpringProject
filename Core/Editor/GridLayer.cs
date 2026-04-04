using System.Collections.Generic;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Editor;

public class GridLayer
{
    public string Name { get; private set; }
    public float ParallaxFactor { get; private set; }
    public bool HasFog { get; private set; }

    public List<LevelObject> LevelObjects { get; private set; }

    public GridLayer(string name, float parallaxFactor, bool hasFog)
    {
        Name = name;
        ParallaxFactor = parallaxFactor;
        HasFog = hasFog;
        
        LevelObjects = new List<LevelObject>();
    }
}