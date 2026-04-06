using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
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

    public ButtonElement(Point position, Point size, Anchor anchor, Texture2D defaultTexture, Texture2D selectedTexture, int cornerSize = 3) : base(position, size, anchor)
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
        UIHelper.DrawSegmented(spriteBatch, _defaultTexture, AbsolutePosition, size, AbsoluteScale, _cornerSize, color);

        if (_selected && !_pressed)
        {
            UIHelper.DrawSegmented(spriteBatch, _selectedTexture, AbsolutePosition, size, AbsoluteScale, _cornerSize, color);
        }

        base.Draw(spriteBatch);

        // Debug.DrawRectangleOutline(spriteBatch, Bounds, Color.Lime, 1);
    }

    public override void OnMouseEnter()
    {
        _selected = true;
        Cursor.BeginHover();
    }

    public override void OnMouseExit()
    {
        _selected = false;
        _pressed = false;

        Cursor.EndHover();
    }

    public override void OnPressed()
    {
        _pressed = true;

        Cursor.BeginPress();
    }

    public override void OnReleased()
    {
        _pressed = false;
        Pressed?.Invoke();

        Cursor.EndPress();
    }
}