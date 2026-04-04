using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core;

public class GridArray : Element
{
    protected Point _gridSize;
    protected int _space;
    
    public GridArray(Point localPosition, Point size, Vector2 localScale, Point gridSize, int space, Origin origin = Origin.MiddleCenter, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, size, localScale, origin, anchor)
    {
        _gridSize = gridSize;
        _space = space;
    }

    public override void AddChild(Element child)
    {
        base.AddChild(child);

        int rowCapacity = ((size.X - _gridSize.X) / (_gridSize.X + _space)) + 1;
        int x = ((_children.Count - 1) % rowCapacity) * (_gridSize.X + _space);
        int y = ((_children.Count - 1) / rowCapacity) * (_gridSize.Y + _space);

        Point pos = new Point(x, y);

        child.SetLocalPosition(pos);

        // recalculate height
        int rows = _children.Count / rowCapacity;
        int height = ((rows + 1)* _gridSize.Y) + (_space * (rows - 1));
        size = new Point(size.X, height);
        ReCalculateBounds();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
}