using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UserInput;
using SpringProject.Core.Editor;
using System.Text.RegularExpressions;

namespace SpringProject.Core.UI;

public enum CursorType
{
    Pointer,
    Pressed,
    Grab,
    EditText,
    BoxSelect,
    Paint,
    Dropper,
    None
}

public static class Cursor
{
    static Texture2D _cursorTexture;
    static Point _cursorOffset;
    public static CursorType CurrentType { get; private set; } = CursorType.None;

    static CursorType _previousType;
    static bool _enabled = false;

    public static void SetCursor(CursorType cursorType)
    {
        if (cursorType == CurrentType) return;

        // if (cursorType == CurrentType)
        // {
        //     Debug.Log($"Cursor type is already '{cursorType}'!");
        // }

        // automatically set the cursor texture based on the cursor type
        CurrentType = cursorType;
        string cursorName = StringUtils.ToSnakeCase(cursorType.ToString());
        _cursorTexture = TextureManager.Get($"cursor_{cursorName}");

        if (cursorType == CursorType.EditText)
        {
            _cursorOffset = new Point(-4, -5);
        }
        else if (cursorType == CursorType.Pointer)
        {
            _cursorOffset = new Point(-4, -2);
        }
        else if (cursorType == CursorType.Pressed)
        {
            _cursorOffset = new Point(-4, -2);
        }
        else if (cursorType == CursorType.Grab)
        {
            _cursorOffset = new Point(-6, -5);
        }
        else if (cursorType == CursorType.BoxSelect)
        {
            _cursorOffset = new Point(-3, -3);
        }
        else if (cursorType == CursorType.Dropper)
        {
            _cursorOffset = new Point(-1, -10);
        }
        else if (cursorType == CursorType.Paint)
        {
            _cursorOffset = new Point(-5, -8);
        }
        else
        {
            _cursorOffset = Point.Zero;
        }
    }

    public static void SetEnabled(bool enabled)
    {
        _enabled = enabled;
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        // do not draw if the cursor is not enabled
        if (!_enabled) return;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Main.UIMatrtix);

        var point = Vector2.Transform(Input.Get("cursor").Vector + _cursorOffset.ToVector2() * Main.Settings.UISize, Matrix.Invert(Main.UIMatrtix));

        spriteBatch.Draw(_cursorTexture, point, Color.White);

        spriteBatch.End();
    }

    public static void Update(GameTime gameTime)
    {
        
    }

    static bool _hovering = false;
    static bool _grabbing = false;

    public static void BeginHover()
    {
        SetCursor(CursorType.Pointer);
        _hovering = true;
    }

    public static void EndHover()
    {
        if (GridPlacement.CurrentTool != null)
        {
            SetCursor(GridPlacement.CurrentTool.CursorType);
        }
        else
        {
            SetCursor(CursorType.Pointer);
        }
        
        _hovering = false;
    }

    public static void BeginPress()
    {
        SetCursor(CursorType.Pressed);
    }

    public static void EndPress()
    {
        if (GridPlacement.CurrentTool != null && !_hovering)
        {
            SetCursor(GridPlacement.CurrentTool.CursorType);
        }
        else
        {
            SetCursor(CursorType.Pointer);
        }
    }

    public static void BeginText()
    {
        SetCursor(CursorType.EditText);
    }

    public static void EndText()
    {
        if (GridPlacement.CurrentTool != null)
        {
            SetCursor(GridPlacement.CurrentTool.CursorType);
        }
        else
        {
            SetCursor(CursorType.Pointer);
        }
    }

    public static void BeginGrab()
    {
        SetCursor(CursorType.Grab);
        _grabbing = true;
    }

    public static void EndGrab()
    {
        if (GridPlacement.CurrentTool != null && !_hovering)
        {
            SetCursor(GridPlacement.CurrentTool.CursorType);
        }
        else
        {
            SetCursor(CursorType.Pointer);
        }
        
        _grabbing = false;
    }
}