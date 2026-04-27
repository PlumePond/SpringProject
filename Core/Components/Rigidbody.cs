using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Components;

public class Rigidbody : Component
{
    public Vector2 ExternalVelocity = Vector2.Zero;
    public Vector2 InternalVelocity = Vector2.Zero;

    public Vector2 Velocity => _velocity;

    Vector2 _velocity = Vector2.Zero;

    [Parameter("Gravity", 0f, 16f)] public int Gravity = 8;
    [Parameter("Bool Test")] public bool BoolTest = false;

    protected Vector2 _position;
    protected Collider _collider;

    public List<Collider> intersectingColliders = new List<Collider>();

    public override void Start()
    {
        _position = LevelObject.transform.position.ToVector2();
        _collider = LevelObject.GetComponent<Collider>();

        if (_collider == null)
        {
            throw new Exception("Rigidbody cannot exist without Collider!");
        }
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        InternalVelocity.Y += Gravity * (float)Main.FIXED_TIMESTEP;

        _velocity = ExternalVelocity + InternalVelocity;

        ResolveCollisions();
        CollisionEnterCheck();
    }

    void CollisionEnterCheck()
    {
        // Expand hitbox by 1 pixel to detect touching (not just overlapping)
        Rectangle expandedHitbox = new Rectangle(
            LevelObject.hitbox.X - 1,
            LevelObject.hitbox.Y - 1,
            LevelObject.hitbox.Width + 2,
            LevelObject.hitbox.Height + 2
        );

        foreach (var component in ComponentSystem.components.ToArray())
        {
            if (component is not Collider) continue;
            if (component == this) continue;

            var collider = (Collider)component;

            if (intersectingColliders.Contains(collider))
            {
                if (!expandedHitbox.Intersects(collider.Bounds))
                {
                    intersectingColliders.Remove(collider);
                    CollisionExit(collider);

                    if (intersectingColliders.Contains(collider))
                    {
                        intersectingColliders.Remove(collider);
                        CollisionExit(collider);
                    }
                }
            }
            else if (expandedHitbox.Intersects(collider.Bounds))
            {
                intersectingColliders.Add(collider);
                CollisionEnter(collider);
                        
                if (!intersectingColliders.Contains(collider))
                {
                    intersectingColliders.Add(collider);
                    CollisionEnter(collider);
                }
            }
        }
    }

    public virtual void CollisionEnter(Collider other)
    {
        _collider.CollisionEnter?.Invoke(other);
        other.CollisionEnter?.Invoke(_collider);
    }

    public virtual void CollisionExit(Collider other)
    {
        _collider.CollisionExit?.Invoke(other);
        other.CollisionExit?.Invoke(_collider);
    }

    void ResolveCollisions()
    {
        Point hitboxOffset = LevelObject.hitbox.Location - LevelObject.transform.position;

        // x axis
        if (_velocity.X != 0)
        {
            Rectangle nextHitboxX = new Rectangle(new Point((int)MathF.Round(_position.X + _velocity.X), (int)MathF.Round(_position.Y)) + hitboxOffset, LevelObject.hitbox.Size);

            foreach (var other in LevelObject.grid.layers[LevelObject.layer].LevelObjects)
            {
                var otherCollider = other.GetComponent<Collider>(); // only objects with a collider
                if (otherCollider == null) continue;
                if (!_collider.CanCollideWith(other)) continue; // skips itself and non-solid objects
                if (!otherCollider.Overlaps(nextHitboxX)) continue;

                if (otherCollider.ResolveX(ref _position, ref ExternalVelocity, ref InternalVelocity, nextHitboxX, hitboxOffset))
                {
                    InternalVelocity.X = 0f;
                    ExternalVelocity.X = 0f;
                    _velocity.X = 0f;
                    break;
                }
            }

            if (_collider.CollidedWithBorders(nextHitboxX))
            {
                if (_velocity.X > 0 && nextHitboxX.Right > LevelObject.size.X * LevelObject.grid.GridSize)
                {
                    InternalVelocity.X = 0;
                    ExternalVelocity.X = 0;
                    _velocity.X = 0;
                    _position.X = LevelObject.grid.size.X * LevelObject.grid.GridSize - LevelObject.hitbox.Width - hitboxOffset.X;
                }
                else if (Velocity.X < 0 && nextHitboxX.Left < 0)
                {
                    InternalVelocity.X = 0;
                    ExternalVelocity.X = 0;
                    _velocity.X = 0;
                    _position.X = 0 - hitboxOffset.X;
                }
            }
        }

        _position.X += _velocity.X;

        // y axis
        if (_velocity.Y != 0)
        {
            Rectangle nextHitboxY = new Rectangle(new Point((int)MathF.Round(_position.X), (int)MathF.Round(_position.Y + _velocity.Y)) + hitboxOffset, LevelObject.hitbox.Size);

            foreach (var other in LevelObject.grid.layers[LevelObject.layer].LevelObjects)
            {
                var otherCollider = other.GetComponent<Collider>(); // only objects with a collider
                if (otherCollider == null) continue;
                if (!_collider.CanCollideWith(other)) continue; // skips itself and non-solid objects
                if (!otherCollider.Overlaps(nextHitboxY)) continue;

                if (otherCollider.ResolveY(ref _position, ref ExternalVelocity, ref InternalVelocity, nextHitboxY, hitboxOffset))
                {
                    InternalVelocity.Y = 0f;
                    ExternalVelocity.Y = 0f;
                    _velocity.Y = 0f;
                    break;
                }
            }
            
            if (_collider.CollidedWithBorders(nextHitboxY))
            {
                if (_velocity.Y > 0 && nextHitboxY.Bottom > LevelObject.grid.size.Y * LevelObject.grid.GridSize)
                {
                    InternalVelocity.Y = 0;
                    ExternalVelocity.Y = 0;
                    _velocity.Y = 0;
                    _position.Y = LevelObject.grid.size.Y * LevelObject.grid.GridSize - LevelObject.hitbox.Height - hitboxOffset.Y;
                }
                else if (_velocity.Y < 0 && nextHitboxY.Top < 0)
                {
                    InternalVelocity.Y = 0;
                    ExternalVelocity.Y = 0;
                    _velocity.Y = 0;
                    _position.Y = 0 - hitboxOffset.Y;
                }
            }
        }

        _position.Y += _velocity.Y;
        LevelObject.SetPosition(new Point((int)MathF.Round(_position.X), (int)MathF.Round(_position.Y)));
    }

    // void ResolveSlopeCollision(LevelObject other, Rectangle nextHitboxY, SlopeCollider slopeCollider)
    // {
    //     Point hitboxOffset = LevelObject.hitbox.Location - LevelObject.transform.position;
        
    //     // Check if we're landing on top of the slope
    //     if (_velocity.Y > 0)
    //     {
    //         float slopeY = slopeCollider.GetSlopeY(nextHitboxY.Center.X);
    //         if (nextHitboxY.Bottom >= slopeY && nextHitboxY.Top < slopeY)
    //         {
    //             _position.Y = slopeY - LevelObject.hitbox.Height - hitboxOffset.Y;
    //             ExternalVelocity.Y = 0;
    //             InternalVelocity.Y = 0;
    //             _velocity.Y = 0;
    //         }
    //     }
    // }
}