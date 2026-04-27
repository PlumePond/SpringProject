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
    SlopeCollider _collider;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        RemoveComponent<BoxCollider>();
        _collider = AddComponent<SlopeCollider>();
    }

    public override void SetFlipX(bool flipX)
    {
        base.SetFlipX(flipX);

        _collider.Direction = flipX ? SlopeDirection.RisingRight : SlopeDirection.RisingLeft;
    }

    public override void CalculateFrameIndex()
    {
        SetFrame(0); // default to the first frame for slopes
    }
}