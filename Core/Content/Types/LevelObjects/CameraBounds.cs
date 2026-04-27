using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Components;
using SpringProject.Core.Content.Types.LevelObjects;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Content.Types;

public class CameraBounds : LevelObject
{
    public override float ResizeDistance => 8;

    protected float _hoverDistance = 6f;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        var collider = AddComponent<Collider>();

        collider.CollisionEnter += OnCollisionEnter;
        collider.CollisionExit += OnCollisionExit;
    }

    public override void OnRemoved()
    {
        GetComponent<Collider>().CollisionEnter -= OnCollisionEnter;
        GetComponent<Collider>().CollisionExit -= OnCollisionExit;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        
    }

    public override void DrawEditor(SpriteBatch spriteBatch)
    {
        Debug.DrawRectangleOutline(spriteBatch, bounds, Color.White);
    }

    void OnCollisionEnter(Collider other)
    {
        if (other == Player.Instance.GetComponent<Collider>())
        {
            GameCamera.Instance.PushBounds(bounds);
        }
    }

    void OnCollisionExit(Collider other)
    {
        if (other == Player.Instance.GetComponent<Collider>())
        {
            GameCamera.Instance.PopBounds(bounds);
        }
    }

    public override bool CanHover(Point mousePos)
    {
        bool containsX = mousePos.X > hitbox.Left && mousePos.X < hitbox.Right;
        bool containsY = mousePos.Y > hitbox.Top && mousePos.Y < hitbox.Bottom;

        if (Math.Abs(hitbox.Left - mousePos.X) < _hoverDistance && containsY)
        {
            return true;
        }
        else if (Math.Abs(hitbox.Right - mousePos.X) < _hoverDistance && containsY)
        {
            return true;
        }
        else if (Math.Abs(hitbox.Top - mousePos.Y) < _hoverDistance && containsX)
        {
            return true;
        }
        else if (Math.Abs(hitbox.Bottom - mousePos.Y) < _hoverDistance && containsX)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void DrawOutline(SpriteBatch spriteBatch)
    {
        Rectangle insideRect = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2);
        Debug.DrawRectangleOutline(spriteBatch, insideRect, Color.Black);

        Rectangle outsideRect = new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height + 2);
        Debug.DrawRectangleOutline(spriteBatch, outsideRect, Color.Black);
    }
}