using System;
using System.ComponentModel.Design;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Content;

namespace SpringProject.Core.UI;

public class ImageElement : Element
{
    string _texture;

    public ImageElement(Point position, Anchor anchor, string texture, Color color) : base(position, TextureManager.Get(texture).Bounds.Size, anchor)
    {
        _texture = texture;
        this.color = color;
    }

    public ImageElement(Point position, Point size, Anchor anchor, string texture) : base(position, size, anchor)
    {
        _texture = texture;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // draw in a rectangle that is defined by the position, size, and scale of the element
        spriteBatch.Draw(TextureManager.Get(_texture), new Rectangle(AbsolutePosition, size * AbsoluteScale.ToPoint()), color);

        base.Draw(spriteBatch);
    }

    public void SetTexture(string texture)
    {
        _texture = texture;
    }
}