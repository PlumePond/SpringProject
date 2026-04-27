using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Components;

public abstract class Collider : Component
{
    public Action<Collider> CollisionEnter;
    public Action<Collider> CollisionExit;

    public Rectangle Bounds => LevelObject.hitbox;

    /// <summary>
    /// Determines whether the Collider should be considered.
    /// </summary>
    public virtual bool CanCollideWith(LevelObject other)
    {
        if (other == LevelObject) return false;
        if (!other.data.solid) return false;
        return true;
    }

    /// <summary>
    /// Does the hitbox overlap even? If so then, yes, check for collisions perchance.
    /// </summary>
    public virtual bool Overlaps(Rectangle movingHitbox)
    {
        return movingHitbox.Intersects(Bounds);
    }

    /// <summary>
    /// Resolves X-axis collision. Returns true if velocity is blocked.
    /// </summary>
    public abstract bool ResolveX(ref Vector2 position, ref Vector2 internalVelocity, ref Vector2 externalVelocity, Rectangle nextHitbox, Point hitboxOffset);

    /// <summary>
    /// Resolves Y-axis collision. Returns true if velocity is blocked.
    /// </summary>
    public abstract bool ResolveY(ref Vector2 position, ref Vector2 internalVelocity, ref Vector2 externalVelocity, Rectangle nextHitbox, Point hitboxOffset);

    /// <summary>
    /// Returns true if the Collider went outside the world's bounds.
    /// </summary>
    public virtual bool CollidedWithBorders(Rectangle hitbox)
    {
        var borderRight = LevelObject.grid.size.X * LevelObject.grid.GridSize;
        var borderLeft = 0;
        var borderTop = 0;
        var borderBottom = LevelObject.grid.size.Y * LevelObject.grid.GridSize;

        return hitbox.Right > borderRight || hitbox.Left < borderLeft || hitbox.Bottom > borderBottom || hitbox.Top < borderTop;
    }
}