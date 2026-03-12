using System;
using System.ComponentModel.Design;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UI;

public class Image : Element
{
    Texture2D _texture;

    public Image(Point position, Vector2 scale, Origin origin, Anchor anchor, Texture2D texture) : base(position, texture.Bounds.Size, scale, origin, anchor)
    {
        _texture = texture;
    }

    public Image(Point position, Vector2 scale, Point size, Origin origin, Anchor anchor, Texture2D texture) : base(position, size, scale, origin, anchor)
    {
        _texture = texture;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        // draw in a rectangle that is defined by the position, size, and scale of the element
        spriteBatch.Draw(_texture, new Rectangle(AbsolutePosition, size * AbsoluteScale.ToPoint()), color);
    }
}