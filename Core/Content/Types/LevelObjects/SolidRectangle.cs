using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Components;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Cont.Types.LevelObjects;

public class SolidRectangle : LevelObject
{
    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        AddComponent<BoxCollider>();
    }
}