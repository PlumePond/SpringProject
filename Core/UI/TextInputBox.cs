using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.UI;

public class TextInputBox : Element
{
    string _text = "";
    string _defaultText;
    Font _font = null;

    Color _defaultColor;
    Point _textSize;

    bool _inputting = false;

    bool _flash = true;
    float _flashTimer = 0.0f;
    float _flashInterval = 2.0f;

    public Action<string> ChangeTextEvent;

    Point _originalSize;

    CursorType _previousCursorType;

    public TextInputBox(Point localPosition, Point size, Font font, string defaultText, Color defaultColor, Color color, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, size, anchor)
    {
        _defaultColor = defaultColor;
        _defaultText = defaultText;
        _font = font;

        this.color = color;

        _originalSize = size;

        _textSize = _font.FontBase.MeasureString(_text, AbsoluteScale).ToPoint();
        //this.size = new Point(_textSize.X, size.Y);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _flashTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_flashTimer > _flashInterval)
        {
            _flash = !_flash;
            _flashTimer = 0.0f;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        //Debug.DrawRectangle(spriteBatch, Bounds, Color.Black);

        if (_text.Length < 1)
        {
            spriteBatch.DrawString(_font.FontBase, _defaultText, AbsolutePosition.ToVector2() + _font.Offset, _defaultColor, 0, Vector2.Zero, AbsoluteScale);
        }
        else
        {
            spriteBatch.DrawString(_font.FontBase, _text, AbsolutePosition.ToVector2() + _font.Offset, color, 0, Vector2.Zero, AbsoluteScale);
        }

        if (_inputting && _flash)
        {
            Debug.DrawRectangle(spriteBatch, new Rectangle(Bounds.Right, Bounds.Top, 1, Bounds.Height), color);
        }

        base.Draw(spriteBatch);
    }

    void OnTextInput(object sender, TextInputEventArgs eventArgs)
    {
        if (eventArgs.Key == Keys.Delete || eventArgs.Key == Keys.Back)
        {
            if (_text.Length > 0)
            {
                SetText(_text.Remove(_text.Length - 1));
            }
        }
        else if (eventArgs.Key == Keys.Enter || eventArgs.Key == Keys.Escape)
        {
            // essentially stop editing
            OnPressedOff();
        }
        else
        {
            SetText(_text + eventArgs.Character);
        }

        // change text event
        ChangeTextEvent?.Invoke(_text);
    }

    public override bool WithinBounds(Point point)
    {
        Rectangle rect = new Rectangle(AbsolutePosition, _originalSize * AbsoluteScale.ToPoint());

        if (rect.Width > Bounds.Width)
        {
            return rect.Contains(point);
        }
        else
        {
            return Bounds.Contains(point);
        }
    }

    public override void OnDisable()
    {
        Main.GameWindow.TextInput -= OnTextInput;
        _inputting = false;
    }

    public override void OnPressed()
    {
        if (!_inputting)
        {
            Main.GameWindow.TextInput += OnTextInput;
            _inputting = true;

            _flash = true;
            _flashTimer = 0.0f;
        }
    }

    public override void OnPressedOff()
    {
        if (_inputting)
        {
            Main.GameWindow.TextInput -= OnTextInput;
            _inputting = false;
        }
    }

    public override void OnMouseEnter()
    {
        base.OnMouseEnter();

        _previousCursorType = Cursor.CurrentType;
        Cursor.SetCursor(CursorType.EditText);
    }

    public override void OnMouseExit()
    {
        base.OnMouseExit();

        Cursor.SetCursor(_previousCursorType);
    }

    public void SetText(string text)
    {
        _text = text;

        // set size only horizontally
        _textSize = _font.FontBase.MeasureString(_text.Length > 0 ? _text : _defaultText, AbsoluteScale).ToPoint();
        size = new Point(_textSize.X, size.Y);
        
        ReCalculateOffsets();
    }
}