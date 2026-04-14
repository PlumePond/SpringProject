using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Audio;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class Slider : Element
{
    string _sliderTexture;
    string _handleTexture;
    string _selectedTexture;
    string _fillTexture;

    int _cornerSize = 16;
    bool _interacting = false;
    bool _selected = false;
    bool _pressed = false;

    float _ratio = 0.0f;
    float _min = 0.0f;
    float _max = 0.0f;
    float _value = 0.0f;

    Point _handleSize = Point.Zero;

    public float Value => _value;

    public Action<float> ChangeValue;

    public Slider(Point position, Point size, Point handleSize, Anchor anchor, string sliderTexture, string handleTexture, string selectedTexture, string fillTexture, float min, float max, float defaultValue, int cornerSize = 3) : base(position, size, anchor)
    {
        _sliderTexture = sliderTexture;
        _handleTexture = handleTexture;
        _selectedTexture = selectedTexture;
        _fillTexture = fillTexture;

        _cornerSize = cornerSize;

        _min = min;
        _max = max;

        _handleSize = handleSize;

        // initialize slider
        _value = defaultValue;
        _ratio = MathUtils.InverseLerp(_min, _max, _value);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Vector2 mousePos = Input.Get("cursor").Vector;
        Vector2 fixedMousePos = new Vector2(mousePos.X / Main.Settings.UISize, mousePos.Y / Main.Settings.UISize);

        float mousePosValue = MathUtils.InverseLerp(AbsolutePosition.X + _handleSize.X / 2, AbsolutePosition.X + size.X - _handleSize.X / 2, fixedMousePos.X);

        if (_interacting)
        {
            _ratio = Math.Clamp(mousePosValue, 0f, 1f);
            _value = MathHelper.Lerp(_min, _max, _ratio);
            // Debug.Log($"Slider value: {_value}.");
            ChangeValue?.Invoke(_value);
        }

        if (Input.Get("ui_click").Released && _interacting || !Main.Graphics.Viewport.Bounds.Contains(mousePos))
        {
            _interacting = false;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        // draw slider
        UIHelper.DrawSegmented(spriteBatch, TextureManager.Get(_sliderTexture), AbsolutePosition, size, AbsoluteScale, _cornerSize, color);

        int handleDistance = (int)MathHelper.Lerp(0.0f, size.X - _handleSize.X, _ratio);
        Point handlePos = new Point(AbsolutePosition.X + handleDistance, AbsolutePosition.Y - (_handleSize.Y - size.Y) / 2);

        // draw fill
        Point baseFillSize = (size.ToVector2() * AbsoluteScale).ToPoint();
        Point fillSize = new Point(handleDistance + _handleSize.X / 2, baseFillSize.Y);
        UIHelper.DrawSegmented(spriteBatch, TextureManager.Get(_fillTexture), AbsolutePosition, fillSize, AbsoluteScale, _cornerSize, color);

        // draw handle
        UIHelper.DrawSegmented(spriteBatch, TextureManager.Get(_handleTexture), handlePos, _handleSize, AbsoluteScale, _cornerSize, color);

        if (_selected && !_pressed)
        {
            UIHelper.DrawSegmented(spriteBatch, TextureManager.Get(_selectedTexture), handlePos, _handleSize, AbsoluteScale, _cornerSize, color);
        }
    }

    public void SetValue(float value)
    {
        _value = Math.Clamp(value, _min, _max);
        _ratio = MathUtils.InverseLerp(_min, _max, _value);
    }

    public override void OnPressed()
    {
        _interacting = true;
        _pressed = true;

        Cursor.BeginGrab();
    }

    public override void OnReleased()
    {
        _pressed = false;

        Cursor.EndGrab();
    }

    public override void OnMouseEnter()
    {
        _selected = true;

        if (_interacting)
        {
            Cursor.BeginGrab();
        }
    }

    public override void OnReleasedOff()
    {
        Cursor.EndGrab();
    }

    public override void OnMouseExit()
    {
        _selected = false;
        _pressed = false;
    }
}