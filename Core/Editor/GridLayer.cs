using System.Collections.Generic;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Editor;

public class GridLayer
{
    List<LevelObject> _levelObjects;

    public List<LevelObject> LevelObjects => _levelObjects;

    public GridLayer()
    {
        _levelObjects = new List<LevelObject>();
    }
}