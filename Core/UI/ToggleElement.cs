using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class ToggleElement : Element
{
    Texture2D _defaultTexture;
    Texture2D _selectedTexture;
    Texture2D _displayTexture;

    int _cornerSize = 16;
    bool _selected = false;
    bool _pressed = false;
    bool _value;

    ImageElement _displayElement;

    public Action<bool> ValueChanged;

    public ToggleElement(Point position, Point size, Anchor anchor, Texture2D defaultTexture, Texture2D selectedTexture, Texture2D displayTexture, bool defaultValue, int cornerSize = 3) : base(position, size, anchor)
    {
        _defaultTexture = defaultTexture;
        _selectedTexture = selectedTexture;
        _displayTexture = displayTexture;

        _cornerSize = cornerSize;
        _value = defaultValue;

        _displayElement = new ImageElement(Point.Zero, Anchor.MiddleCenter, _displayTexture, Main.UIDefaultColor);
        AddChild(_displayElement);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        UIHelper.DrawSegmented(spriteBatch, _defaultTexture, AbsolutePosition, size, AbsoluteScale, _cornerSize, color);
        if (_selected && !_pressed)
        {
            UIHelper.DrawSegmented(spriteBatch, _selectedTexture, AbsolutePosition, size, AbsoluteScale, _cornerSize, color);
        }

        base.Draw(spriteBatch);
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

        Toggle();
    }

    public void Toggle()
    {
        _value = !_value;
        ValueChanged?.Invoke(_value);
        _displayElement.SetColor(_value ? Main.UIEnabledColor : Main.UIDefaultColor);
    }
}