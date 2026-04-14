using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.AI;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Editor;

public class Entity : LevelObject
{
    public Animator Animator { get; protected set; }
    public StateMachine StateMachine { get; protected set; }

    public bool Grounded { get; protected set; } = false;
    public Material FootstepMaterial { get; protected set; }

    public Vector2 Velocity;

    protected virtual float Gravity => 8.0f;
    protected virtual float PathfnderUpdateInterval => 0.5f;
    protected virtual float NodeThresholdX => 16f;
    protected virtual float NodeThresholdY => 32f;
    protected virtual Point GroundCheckSize => new Point(data.hitbox.Width, 3);
    
    protected Vector2 _position;
    protected float _pathfinderUpdateTimer = 0.0f;
    protected List<Node> _path;
    protected int _currentNode = 0;
    
    protected Rectangle _groundCheck;
    protected Transform _target = null;

    public List<Entity> intersectingEntities = new List<Entity>();

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);
        Animator = new Animator(this);
        StateMachine = new StateMachine(this);

        Velocity = Vector2.Zero;
        _position = hitbox.Location.ToVector2();

        Animator.IterateFrame += OnIterateFrame;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Animator.Update(gameTime);
        StateMachine.Update(gameTime);
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Velocity.Y += Gravity * deltaTime;

        ResolveCollisions();
        GroundedCheck();
        EntityEnterCheck();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Animator.Draw(spriteBatch);
        StateMachine.Draw(spriteBatch);
    }

    protected virtual void GroundedCheck()
    {
        _groundCheck = new Rectangle(hitbox.Center.X - GroundCheckSize.X / 2, hitbox.Bottom, GroundCheckSize.X, GroundCheckSize.Y);

        if (grid.RectInsideObject(_groundCheck, layer, out var levelObject, this))
        {
            Grounded = true;
            FootstepMaterial = levelObject.data.material;
        }
        else
        {
            Grounded = false;
        }
  
        if (_groundCheck.Bottom > grid.size.Y * 16)
        {
            Grounded = true;
        }
    }

    protected virtual void OnIterateFrame(int frame)
    {
        StateMachine.CurrentState?.IterateFrame(frame);
    }

    protected virtual void HandlePathfinding(GameTime gameTime)
    {
        if (_target == null) return;

        _pathfinderUpdateTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_pathfinderUpdateTimer > PathfnderUpdateInterval)
        {
            var path = Pathfinder.FindPath(hitbox.Center, _target.position);

            if (path != null && path.Count > 0)
            {
                _path = path;
                _currentNode = 0;

                // find the closest node to current position to avoid snapping back
                float closest = float.MaxValue;
                for (int i = 0; i < _path.Count; i++)
                {
                    float dist = hitbox.Center.Distance(_path[i].Point);
                    if (dist < closest)
                    {
                        closest = dist;
                        _currentNode = i;
                    }
                }

                _pathfinderUpdateTimer = 0.0f;
            }
        }

        if (_path != null && _path.Count > 1)
        {
            FollowPath();
            
            bool withinThresholdX = MathF.Abs(hitbox.Center.X - _path[_currentNode].Point.X) < NodeThresholdX;
            bool withinThresholdY = MathF.Abs(hitbox.Center.Y - _path[_currentNode].Point.Y) < NodeThresholdY;

            if (withinThresholdX || withinThresholdY)
            {
                if (_currentNode + 1 < _path.Count)
                {
                    _currentNode++;
                }
            }
        }
    }

    protected virtual void FollowPath()
    {
        
    }

    protected void ResolveCollisions()
    {
        Point hitboxOffset = hitbox.Location - transform.position;

        // X axis
        if (Velocity.X != 0)
        {
            Rectangle nextHitboxX = new Rectangle(
                new Point((int)MathF.Round(_position.X + Velocity.X), (int)MathF.Round(_position.Y)) + hitboxOffset,
                hitbox.Size);

            foreach (var levelObject in grid.layers[layer].LevelObjects)
            {
                if (!Collided(levelObject, nextHitboxX)) continue;

                if (Velocity.X > 0 && levelObject.hitbox.Left >= hitbox.Right - 2)
                {
                    _position.X = levelObject.hitbox.Left - hitbox.Width - hitboxOffset.X;
                    Velocity.X = 0;
                    break;
                }
                else if (Velocity.X < 0 && levelObject.hitbox.Right <= hitbox.Left + 2)
                {
                    _position.X = levelObject.hitbox.Right - hitboxOffset.X;
                    Velocity.X = 0;
                    break;
                }
            }

            if (CollidedWithBorders(nextHitboxX))
            {
                int gridSize = 16;
                if (Velocity.X > 0)
                {
                    Velocity.X = 0;
                    _position.X = grid.size.X * gridSize - hitbox.Width - hitboxOffset.X;
                }
                else if (Velocity.X < 0)
                {
                    Velocity.X = 0;
                    _position.X = 0 - hitboxOffset.X;
                }
            }
        }

        _position.X += Velocity.X;

        // Y axis
        if (Velocity.Y != 0)
        {
            Rectangle nextHitboxY = new Rectangle(
                new Point((int)MathF.Round(_position.X), (int)MathF.Round(_position.Y + Velocity.Y)) + hitboxOffset,
                hitbox.Size);

            foreach (var levelObject in grid.layers[layer].LevelObjects)
            {
                if (!Collided(levelObject, nextHitboxY)) continue;

                if (Velocity.Y > 0 && levelObject.hitbox.Top >= hitbox.Bottom - 2)
                {
                    _position.Y = levelObject.hitbox.Top - hitbox.Height - hitboxOffset.Y;
                    Velocity.Y = 0;
                    break;
                }
                else if (Velocity.Y < 0 && levelObject.hitbox.Bottom <= hitbox.Top + 2)
                {
                    _position.Y = levelObject.hitbox.Bottom - hitboxOffset.Y;
                    Velocity.Y = 0;
                    break;
                }
            }
            
            if (CollidedWithBorders(nextHitboxY))
            {
                int gridSize = 16;
                if (Velocity.Y > 0)
                {
                    Velocity.Y = 0;
                    _position.Y = grid.size.Y * gridSize - hitbox.Height - hitboxOffset.Y;
                }
                else if (Velocity.Y < 0)
                {
                    Velocity.Y = 0;
                    _position.Y = 0 - hitboxOffset.Y;
                }
            }
        }

        _position.Y += Velocity.Y;
        SetPosition(new Point((int)MathF.Round(_position.X), (int)MathF.Round(_position.Y)));
    }

    protected virtual bool Collided(LevelObject levelObject, Rectangle hitbox)
    {
        if (levelObject == this) return false;
        if (!levelObject.data.solid) return false;
        if (!hitbox.Intersects(levelObject.hitbox)) return false;

        return true;
    }

    protected virtual bool CollidedWithBorders(Rectangle hitbox)
    {
        int gridSize = 16;

        if (hitbox.Bottom > grid.size.Y * gridSize)
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
        if (hitbox.Right > grid.size.X * gridSize)
        {
            return true;
        }

        return false;
    }

    void EntityEnterCheck()
    {
        foreach (var levelObject in grid.layers[layer].LevelObjects)
        {
            if (levelObject != this && levelObject.GetType().IsSubclassOf(typeof(Entity)))
            {
                Entity entity = (Entity)levelObject;

                if (intersectingEntities.Contains(entity))
                {
                    if (!hitbox.Intersects(entity.hitbox))
                    {
                        intersectingEntities.Remove(entity);
                        OnEntityExit(entity);

                        if (entity.intersectingEntities.Contains(this))
                        {
                            entity.intersectingEntities.Remove(this);
                            entity.OnEntityExit(this);
                        }
                    }
                }
                else if (hitbox.Intersects(levelObject.hitbox))
                {
                    intersectingEntities.Add(entity);
                    OnEntityEnter(entity);
                    
                    if (!entity.intersectingEntities.Contains(this))
                    {
                        entity.intersectingEntities.Add(this);
                        entity.OnEntityEnter(this);
                    }
                }
            }
        }
    }

    public virtual void OnEntityEnter(Entity other)
    {
        
    }

    public virtual void OnEntityExit(Entity other)
    {
        
    }

    public override void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        base.DrawDebug(spriteBatch, font);

        Debug.DrawRectangle(spriteBatch, _groundCheck, Grounded ? Color.Green : Color.Red);

        if (Velocity.Length() > 0.5f)
        {
            Vector2 direction = Vector2.Normalize((hitbox.Center.ToVector2() + Velocity - hitbox.Center.ToVector2()));
            float length = 32f;
            Point extrapolated = (hitbox.Center.ToVector2() + (direction * length)).ToPoint();
            Debug.DrawLine(spriteBatch, hitbox.Center, extrapolated, Color.Yellow, 1);
        }

        StateMachine.CurrentState?.DrawDebug(spriteBatch);
    }
}