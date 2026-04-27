using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Components;

public class Collider : Component
{
    public Action<Collider> CollisionEnter;
    public Action<Collider> CollisionExit;

    public Rectangle Bounds => LevelObject.hitbox;

    public virtual bool Collided(LevelObject levelObject, Rectangle hitbox)
    {
        if (levelObject == LevelObject) return false;
        if (!levelObject.data.solid) return false;
        if (!hitbox.Intersects(levelObject.hitbox)) return false;

        return true;
    }

    public virtual bool CollidedWithBorders(Rectangle hitbox)
    {
        int gridSize = 16;

        if (hitbox.Bottom > LevelObject.grid.size.Y * gridSize)
        {
            return true;
        }
        if (hitbox.Top < 0)
        {
            return true;
        }
        if (hitbox.Left < 0)
        {
            return true;
        }
        if (hitbox.Right > LevelObject.grid.size.X * gridSize)
        {
            return true;
        }

        return false;
    }
}