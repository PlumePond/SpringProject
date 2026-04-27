using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.UI;

public enum ArrayDirection
{
    Right,
    Left,
    Down,
    Up
}

public class ArrayElement : Element
{
    protected int _space;
    protected ArrayDirection _direction;

    public Action<Point> UpdateSizeEvent;

    public ArrayElement(Point position, Point size, int space, ArrayDirection direction, Anchor anchor) : base(position, size, anchor)
    {
        _space = space;
        _direction = direction;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        // Debug.DrawRectangleOutline(spriteBatch, Bounds, Color.Yellow);
    }

    public override void AddChild(Element child)
    {
        base.AddChild(child);

        int step = 0;
        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].SetLocalPosition(GetLocalPosition(_children[i], step));
            step += GetStepSize(_children[i]) + _space;
        }

        RecalculateSize();
    }

    public override void RemoveChild(Element child)
    {
        base.RemoveChild(child);

        int step = 0;
        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].SetLocalPosition(GetLocalPosition(_children[i], step));
            step += GetStepSize(_children[i]) + _space;
        }

        RecalculateSize();
    }

    public void RecalculateSize()
    {
        int mainAxis = 0;
        int crossAxis = 0;

        foreach (var child in _children)
        {
            mainAxis += GetStepSize(child) + _space;
            crossAxis = Math.Max(crossAxis, GetCrossSize(child));
        }

        if (_children.Count > 0)
        {
            mainAxis -= _space;
        }

        switch (_direction)
        {
            case ArrayDirection.Right or ArrayDirection.Left: size = new Point(mainAxis, crossAxis); break;
            case ArrayDirection.Down or ArrayDirection.Up: size = new Point(crossAxis, mainAxis); break;
        }

        ReCalculateOffsets();
        UpdateSizeEvent?.Invoke(size);
    }

    int GetStepSize(Element child)
    {
        switch (_direction)
        {
            case ArrayDirection.Right or ArrayDirection.Left: return child.size.X;
            case ArrayDirection.Up or ArrayDirection.Down:   return child.size.Y;
            default: return 0;
        }
    }

    int GetCrossSize(Element child)
    {
        switch (_direction)
        {
            case ArrayDirection.Right or ArrayDirection.Left: return child.size.Y;
            case ArrayDirection.Down  or ArrayDirection.Up: return child.size.X;
            default: return 0;
        }
    }

    Point GetLocalPosition(Element child, int step)
    {
        switch (_direction)
        {
            case ArrayDirection.Right: return new Point(step, 0);
            case ArrayDirection.Left: return new Point(-step, 0);
            case ArrayDirection.Down: return new Point(0, step);
            case ArrayDirection.Up: return new Point(0, -step);
            default: return Point.Zero;
        }
    }
}