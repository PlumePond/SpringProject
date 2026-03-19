using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class ToggleElement : Element
{
    Texture2D _defaultTexture;
    Texture2D _selectedTexture;
    Texture2D _displayTrueTexture;
    Texture2D _displayFalseTexture;

    int _cornerSize = 16;
    bool _selected = false;
    bool _pressed = false;
    bool _value;

    ImageElement _displayElement;

    public Action<bool> ValueChanged;

    public ToggleElement(Point position, Point size, Vector2 scale, Origin origin, Anchor anchor, Texture2D defaultTexture, Texture2D selectedTexture, Texture2D displayTrueTexture, Texture2D displayFalseTexture, bool defaultValue, int cornerSize = 16) : base(position, size, scale, origin, anchor)
    {
        _defaultTexture = defaultTexture;
        _selectedTexture = selectedTexture;
        _displayTrueTexture = displayTrueTexture;
        _displayFalseTexture = displayFalseTexture;

        _cornerSize = cornerSize;
        _value = defaultValue;

        _displayElement = new ImageElement(Point.Zero, scale, Origin.MiddleCenter, Anchor.MiddleCenter, _value ? _displayTrueTexture : _displayFalseTexture);
        AddChild(_displayElement);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
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

        Toggle();
    }

    public void Toggle()
    {
        _value = !_value;
        ValueChanged?.Invoke(_value);
        _displayElement.SetTexture(_value ? _displayTrueTexture : _displayFalseTexture);
    }
}