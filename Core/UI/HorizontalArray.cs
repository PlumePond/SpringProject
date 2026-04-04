using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UI;

public class HorizontalArray : Element
{
    protected int _space;

    public HorizontalArray(Point position, Point size, Vector2 scale, int space) : base(position, size, scale, Origin.MiddleLeft, Anchor.MiddleLeft)
    {
        _space = space;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }

    public override void AddChild(Element child)
    {
        base.AddChild(child);

        child.SetLocalPosition(new Point(_children.Count * (child.size.X + _space) - child.size.X - _space, 0));
    }
}