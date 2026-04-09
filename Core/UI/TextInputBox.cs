using System;
using System.Reflection;
using System.Reflection.PortableExecutable;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UserInput;
using TextCopy;

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

    bool _dragging = false;
    Point _pressPos;
    Point _currentPos;

    int _cursorIndex = 0;
    int _selectionStart = -1;
    int _selectionEnd = -1;

    float _leftHeldTimer = 0f;
    float _rightHeldTimer = 0f;
    const float KeyRepeatDelay = 1.5f;
    const float KeyRepeatInterval = 0.2f;

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

        if (_hovering)
        {
            Input.ConsumeHover();
        }        

        if (_dragging)
        {
            _currentPos = Vector2.Transform(Input.Get("cursor").Point.ToVector2(), Matrix.Invert(Main.UIMatrtix)).ToPoint();

            _selectionEnd = GetIndexAt(_currentPos.X);
            _cursorIndex = _selectionEnd;
        }

        if (Input.Get("text_left").Holding)
        {
            _leftHeldTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_leftHeldTimer == (float)gameTime.ElapsedGameTime.TotalSeconds || // first frame
                _leftHeldTimer > KeyRepeatDelay && (_leftHeldTimer % KeyRepeatInterval) < (float)gameTime.ElapsedGameTime.TotalSeconds)
            {
                _cursorIndex = Math.Max(0, _cursorIndex - 1);
                _selectionStart = _cursorIndex;
                _selectionEnd = _cursorIndex;
                _flash = true;
                _flashTimer = 0f;
            }
        }
        else _leftHeldTimer = 0f;

        if (Input.Get("text_right").Holding)
        {
            _rightHeldTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_rightHeldTimer == (float)gameTime.ElapsedGameTime.TotalSeconds ||
                _rightHeldTimer > KeyRepeatDelay && (_rightHeldTimer % KeyRepeatInterval) < (float)gameTime.ElapsedGameTime.TotalSeconds)
            {
                _cursorIndex = Math.Min(_text.Length, _cursorIndex + 1);
                _selectionStart = _cursorIndex;
                _selectionEnd = _cursorIndex;
                _flash = true;
                _flashTimer = 0f;
            }
        }
        else _rightHeldTimer = 0f;
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

        if (_inputting && Input.Get("paste").Pressed)
        {
            Paste();
        }

        if (_inputting && Input.Get("copy").Pressed)
        {
            Copy();
        }
        if (_inputting && Input.Get("cut").Pressed)
        {
            Copy();
            DeleteSelection();
        }

        // selection highlight
        if (_inputting && _selectionStart != _selectionEnd)
        {
            int selLeft = Math.Min(_selectionStart, _selectionEnd);
            int selRight = Math.Max(_selectionStart, _selectionEnd);

            float startX = _font.FontBase.MeasureString(_text[..selLeft], AbsoluteScale).X;
            float endX = _font.FontBase.MeasureString(_text[..selRight], AbsoluteScale).X;

            startX = Math.Clamp(startX, 0, Bounds.Width);
            endX = Math.Clamp(endX, 0, Bounds.Width);

            var highlightRect = new Rectangle(
                (int)(AbsolutePosition.X + startX),
                Bounds.Top,
                (int)(endX - startX),
                Bounds.Height
            );
            Debug.DrawRectangle(spriteBatch, highlightRect, Color.LightBlue * 0.5f);
        }

        // cursor blink at cursor index
        if (_inputting && _flash)
        {
            float cursorX = _font.FontBase.MeasureString(_text[.._cursorIndex], AbsoluteScale).X;
            Debug.DrawRectangle(spriteBatch, new Rectangle(
                (int)(AbsolutePosition.X + cursorX),
                Bounds.Top, 1, Bounds.Height), color);
        }

        base.Draw(spriteBatch);
    }

    int GetIndexAt(int worldX)
    {
        var localX = worldX - AbsolutePosition.X;
        for (int i = 0; i <= _text.Length; i++)
        {
            float charX = _font.FontBase.MeasureString(_text[..i], AbsoluteScale).X;

            if (localX <= charX)
            {
                return i;
            }
        }
        return _text.Length;
    }

    void Copy()
    {
        if (HasSelection())
        {
            int selLeft = Math.Min(_selectionStart, _selectionEnd);
            int selRight = Math.Max(_selectionStart, _selectionEnd);
            ClipboardService.SetText(_text[selLeft..selRight]);
        }
        else
        {
            ClipboardService.SetText(_text);
        }
    }

    void Paste()
    {
        var clipBoardText = ClipboardService.GetText();
        SetText(_text + clipBoardText);
    }

    void OnTextInput(object sender, TextInputEventArgs eventArgs)
    {
        if (eventArgs.Key == Keys.Delete || eventArgs.Key == Keys.Back)
        {
            if (HasSelection())
            {
                DeleteSelection();
            }
            else if (_cursorIndex > 0)
            {
                SetText(_text.Remove(_cursorIndex - 1, 1));
                _cursorIndex--;
            }
        }
        else if (eventArgs.Key == Keys.Enter || eventArgs.Key == Keys.Escape)
        {
            OnPressedOff();
        }
        else
        {
            if (HasSelection()) DeleteSelection();
            SetText(_text.Insert(_cursorIndex, eventArgs.Character.ToString()));
            _cursorIndex++;
        }

        _selectionStart = _cursorIndex;
        _selectionEnd = _cursorIndex;
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

    bool HasSelection()
    {
        return _selectionStart != _selectionEnd;
    }

    void DeleteSelection()
    {
        int selLeft = Math.Min(_selectionStart, _selectionEnd);
        int selRight = Math.Max(_selectionStart, _selectionEnd);
        SetText(_text.Remove(selLeft, selRight - selLeft));
        _cursorIndex = selLeft;
        _selectionStart = _cursorIndex;
        _selectionEnd = _cursorIndex;
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
            
            // lock input
            Input.SetLocked(true);
        }

        var worldPos = Vector2.Transform(Input.Get("cursor").Point.ToVector2(), Matrix.Invert(Main.UIMatrtix)).ToPoint();
        _cursorIndex = GetIndexAt(worldPos.X);
        _selectionStart = _cursorIndex;
        _selectionEnd = _cursorIndex;
        _pressPos = worldPos;
        _dragging = true;

        Input.ConsumePress();
    }

    public override void OnReleased()
    {
        Cursor.BeginText();
        _dragging = false;
    }

    public override void OnReleasedOff()
    {
        _dragging = false;
    }

    public override void OnPressedOff()
    {
        if (_inputting)
        {
            Main.GameWindow.TextInput -= OnTextInput;
            _inputting = false;

            // unlock input
            Input.SetLocked(false);
        }
    }

    public override void OnMouseEnter()
    {
        base.OnMouseEnter();

        Cursor.BeginText();
    }

    public override void OnMouseExit()
    {
        base.OnMouseExit();

        Cursor.EndText();
    }

    public void SetText(string text)
    {
        _text = text;

        // set size only horizontally
        _textSize = _font.FontBase.MeasureString(_text.Length > 0 ? _text : _defaultText, AbsoluteScale).ToPoint();
        size = new Point(_textSize.X, size.Y);

        // change text event
        ChangeTextEvent?.Invoke(_text);
        
        ReCalculateOffsets();
    }
}