using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.AI;
using SpringProject.Core.Components;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Editor;

public class Entity : LevelObject
{
    public Animator Animator { get; protected set; }

    public bool Grounded { get; protected set; } = false;
    public Material FootstepMaterial { get; protected set; }

    protected virtual float Gravity => 8.0f;
    protected virtual Point GroundCheckSize => new Point(data.hitbox.Width, 3);
    
    protected Rectangle _groundCheck;
    protected Transform _target = null;

    Rigidbody _rigidBody;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);
        AddComponent<Collider>();
        _rigidBody = AddComponent<Rigidbody>();

        Animator = AddComponent<Animator>();
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        GroundedCheck();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
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

    public override void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        base.DrawDebug(spriteBatch, font);

        Debug.DrawRectangle(spriteBatch, _groundCheck, Grounded ? Color.Green : Color.Red);

        if (_rigidBody.Velocity.Length() > 0.5f)
        {
            Vector2 direction = Vector2.Normalize((hitbox.Center.ToVector2() + _rigidBody.Velocity - hitbox.Center.ToVector2()));
            float length = 32f;
            Point extrapolated = (hitbox.Center.ToVector2() + (direction * length)).ToPoint();
            Debug.DrawLine(spriteBatch, hitbox.Center, extrapolated, Color.Yellow, 1);
        }
    }
}