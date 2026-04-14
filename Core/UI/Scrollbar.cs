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

public class Scrollbar : Element
{
    string _sliderTexture;
    string _handleTexture;
    string _selectedTexture;
    string _fillTexture;

    int _cornerSize;
    bool _interacting = false;
    bool _selected = false;
    bool _pressed = false;

    float _ratio = 0.0f;
    float _min = 0.0f;
    float _max = 0.0f;
    float _value = 0.0f;

    Point _handleSize;

    public float Value => _value;

    public Action<float> ChangeValueEvent;

    bool _canScroll = true;

    public Scrollbar(Point position, Point size, Anchor anchor, string sliderTexture, string handleTexture, string selectedTexture, string fillTexture, float min, float max, float defaultValue, int cornerSize = 2) : base(position, size, anchor)
    {
        _sliderTexture = sliderTexture;
        _handleTexture = handleTexture;
        _selectedTexture = selectedTexture;
        _fillTexture = fillTexture;

        _cornerSize = cornerSize;

        _handleSize = TextureManager.Get(handleTexture).Bounds.Size;

        _min = min;
        _max = max;

        // initialize slider
        _value = defaultValue;
        _ratio = MathUtils.InverseLerp(_min, _max, _value);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Point mousePoint = Input.Get("cursor").Point;
        Point fixedMousePoint = new Point(mousePoint.X / Main.Settings.UISize, mousePoint.Y / Main.Settings.UISize);

        float mousePosValue = MathUtils.InverseLerp(AbsolutePosition.Y + _handleSize.Y / 2, AbsolutePosition.Y + size.Y - _handleSize.Y / 2, fixedMousePoint.Y);

        if (_interacting && _canScroll)
        {
            _ratio = Math.Clamp(mousePosValue, 0f, 1f);
            _value = MathHelper.Lerp(_min, _max, _ratio);
            ChangeValueEvent?.Invoke(_value);
            Input.ConsumeHover();
            Input.ConsumePress();
        }

        if (Input.Get("ui_click").Released && _interacting || !Main.Graphics.Viewport.Bounds.Contains(mousePoint))
        {
            _interacting = false;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        Rectangle topLeft = new Rectangle(0, 0, 2, 2);
        Rectangle top = new Rectangle(3, 0, 2, 2);
        Rectangle topRight = new Rectangle(6, 0, 2, 2);

        Rectangle left = new Rectangle(0, 3, 2, 2);
        Rectangle mid = new Rectangle(3, 3, 2, 2);
        Rectangle right = new Rectangle(6, 3, 2, 2);
        
        Rectangle bottomLeft = new Rectangle(0, 6, 2, 3);
        Rectangle bottom = new Rectangle(3, 6, 2, 3);
        Rectangle bottomRight = new Rectangle(6, 6, 2, 3);

        // draw slider
        UIHelper.DrawSegmentedRepeating(spriteBatch, TextureManager.Get(_sliderTexture), new Rectangle(AbsolutePosition, size), topLeft, top, topRight, left, mid, right, bottomLeft, bottom, bottomRight);

        // lerp over size.Y, offset handle on Y axis
        int handleDistance = (int)MathHelper.Lerp(0.0f, size.Y - _handleSize.Y, _ratio);
        Point handlePos = new Point(AbsolutePosition.X, AbsolutePosition.Y + handleDistance);

        if (_fillTexture != null)
        {
            // fill grows downward
            Point baseFillSize = (size.ToVector2() * AbsoluteScale).ToPoint();
            Point fillSize = new Point(baseFillSize.X, handleDistance + _handleSize.Y / 2);
            UIHelper.DrawSegmented(spriteBatch, TextureManager.Get(_fillTexture), AbsolutePosition, fillSize, AbsoluteScale, _cornerSize, color);
        }

        // if it cannot scroll, dont even bother rendering
        if (!_canScroll) return;

        // draw handle
        UIHelper.DrawSegmented(spriteBatch, TextureManager.Get(_handleTexture), handlePos, _handleSize, AbsoluteScale, _cornerSize, color);

        if (_selected && !_pressed)
        {
            UIHelper.DrawSegmented(spriteBatch, TextureManager.Get(_selectedTexture), handlePos, _handleSize, AbsoluteScale, _cornerSize, color);
        }
    }

    public void SetCanScroll(bool value)
    {
        _canScroll = value;
    }

    public void SetValue(float value)
    {
        _value = Math.Clamp(value, _min, _max);
        _ratio = MathUtils.InverseLerp(_min, _max, _value);
    }

    public override void OnPressed()
    {
        // ignore if cannot scroll
        if (!_canScroll) return;

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
        // ignore if cannot scroll
        if (!_canScroll) return;
        
        _selected = true;

        if (_interacting)
        {
            Cursor.BeginGrab();
        }
    }

    public override void OnMouseHover()
    {
        Input.ConsumeHover();
    }

    public override void OnMouseExit()
    {
        _selected = false;
        _pressed = false;
    }

    public override void OnReleasedOff()
    {
        Cursor.EndGrab();
    }
}