using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Components;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Cont.Types.LevelObjects;

public class SlopeTile : Tile
{
    public override void CalculateFrameIndex()
    {
        SetFrame(0); // default to the first frame for slopes
    }
}