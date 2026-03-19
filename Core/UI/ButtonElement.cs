using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class ButtonElement : Element
{
    Texture2D _defaultTexture;
    Texture2D _selectedTexture;

    int _cornerSize = 16;
    bool _selected = false;
    bool _pressed = false;

    public Action Pressed;

    public ButtonElement(Point position, Point size, Vector2 scale, Origin origin, Anchor anchor, Texture2D defaultTexture, Texture2D selectedTexture, int cornerSize = 16) : base(position, size, scale, origin, anchor)
    {
        _defaultTexture = defaultTexture;
        _selectedTexture = selectedTexture;
        _cornerSize = cornerSize;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Point mousePoint = Input.Get("cursor").Point;
        Point fixedMousePoint = new Point(mousePoint.X / Main.Settings.UISize, mousePoint.Y / Main.Settings.UISize);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        
        Texture2D texture = _defaultTexture;

        if (_selected && !_pressed)
        {
            texture = _selectedTexture;
        }

        UIHelper.DrawSegmented(spriteBatch, texture, AbsolutePosition, size, AbsoluteScale, _cornerSize, color);
    }

    public override void OnMouseEnter()
    {
        _selected = true;
    }

    public override void OnMouseExit()
    {
        _selected = false;
        _pressed = false;
    }

    public override void OnPressed()
    {
        _pressed = true;
    }

    public override void OnReleased()
    {
        _pressed = false;
        Pressed?.Invoke();
    }
}