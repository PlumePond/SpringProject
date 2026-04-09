using System;
using System.Collections.Generic;
using System.ComponentModel;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

    protected float _gravity;
    protected Vector2 _position;

    protected Point _groundCheckSize;
    protected Rectangle _groundCheck;

    public List<Entity> intersectingEntities = new List<Entity>();

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);
        Animator = new Animator(this);
        StateMachine = new StateMachine(this);

        Velocity = Vector2.Zero;
        _position = hitbox.Location.ToVector2();
        _gravity = 8.0f;
        _groundCheckSize = new Point(data.frame.X - 4, 3);

        Animator.IterateFrame += OnIterateFrame;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Animator.Update(gameTime);
        StateMachine.Update(gameTime);
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Velocity.Y += _gravity * deltaTime;

        ResolveCollisions();
        GroundedCheck();
        EntityEnterCheck();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Animator.Draw(spriteBatch);
    }

    void GroundedCheck()
    {
        Grounded = false;
        _groundCheck = new Rectangle((int)_position.X + _groundCheckSize.X / 4, (int)_position.Y + hitbox.Height, _groundCheckSize.X, _groundCheckSize.Y);

        foreach (var levelObject in grid.layers[layer].LevelObjects)
        {
            if (levelObject != this && levelObject.data.solid)
            {
                if (levelObject.hitbox.Intersects(_groundCheck))
                {
                    Grounded = true;
                    FootstepMaterial = levelObject.data.material;
                    break;
                }
            }
        }
    }

    protected virtual void OnIterateFrame(int frame)
    {
        StateMachine.CurrentState?.IterateFrame(frame);
    }

    protected void ResolveCollisions()
    {
        // calculate the offset between _position and the hitbox position
        Point hitboxOffset = hitbox.Location - transform.position;

        // x axis
        int roundedY = (int)MathF.Round(_position.Y);
        Point collisionPosX = new Point((int)MathF.Round(_position.X + Velocity.X), roundedY);
        Rectangle collisionRectX = new Rectangle(collisionPosX + hitboxOffset, hitbox.Size);

        foreach (var levelObject in grid.layers[layer].LevelObjects)
        {
            if (levelObject != this && levelObject.data.solid)
            {
                if (collisionRectX.Intersects(levelObject.hitbox))
                {
                    if (Velocity.X > 0)
                        _position.X = levelObject.hitbox.Left - hitbox.Width - hitboxOffset.X;
                    else if (Velocity.X < 0)
                        _position.X = levelObject.hitbox.Right - hitboxOffset.X;

                    Velocity.X = 0;
                }
            }
        }

        _position.X += Velocity.X;

        // y axis
        int roundedX = (int)MathF.Round(_position.X);
        Point collisionPosY = new Point(roundedX, (int)MathF.Round(_position.Y + Velocity.Y));
        Rectangle collisionRectY = new Rectangle(collisionPosY + hitboxOffset, hitbox.Size);

        foreach (var levelObject in grid.layers[layer].LevelObjects)
        {
            if (levelObject != this && levelObject.data.solid)
            {
                if (collisionRectY.Intersects(levelObject.hitbox))
                {
                    if (Velocity.Y > 0)
                        _position.Y = levelObject.hitbox.Top - hitbox.Height - hitboxOffset.Y;
                    else if (Velocity.Y < 0)
                        _position.Y = levelObject.hitbox.Bottom - hitboxOffset.Y;

                    Velocity.Y = 0;
                }
            }
        }

        _position.Y += Velocity.Y;
        SetPosition(_position.ToPoint());
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
    }
}