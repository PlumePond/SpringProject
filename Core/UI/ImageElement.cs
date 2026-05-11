using System;
using System.ComponentModel.Design;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Content;

namespace SpringProject.Core.UI;

public class ImageElement : Element
{
    string _texturePath;
    Texture2D _texture = null;

    bool _useTextureDirectly = false;

    public ImageElement(Point position, Anchor anchor, string texturePath, Color color) : base(position, TextureManager.Get(texturePath).Bounds.Size, anchor)
    {
        _texturePath = texturePath;
        this.color = color;
    }

    public ImageElement(Point position, Point size, Anchor anchor, string texturePath) : base(position, size, anchor)
    {
        _texturePath = texturePath;
    }

    public ImageElement(Point position, Point size, Anchor anchor, Texture2D texture) : base(position, size, anchor)
    {
        _texture = texture;
        _useTextureDirectly = true;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // draw in a rectangle that is defined by the position, size, and scale of the element
        if (_useTextureDirectly)
        {
            if (_texture != null)
            {
                spriteBatch.Draw(_texture, new Rectangle(AbsolutePosition, size * AbsoluteScale.ToPoint()), color);
            }
        }
        else
        {
            spriteBatch.Draw(TextureManager.Get(_texturePath), new Rectangle(AbsolutePosition, size * AbsoluteScale.ToPoint()), color);
        }

        base.Draw(spriteBatch);
    }

    public void SetTexturePath(string texturePath)
    {
        _texturePath = texturePath;
    }

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }
}