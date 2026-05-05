using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Editor;

public class BoxSelectTool(GridPlacement placement) : Tool(placement)
{
    public override CursorType CursorType => CursorType.BoxSelect;

    bool _selecting = false;
    Rectangle _selectionRect;
    Point _rectStart;

    public override void Update(GameTime gameTime)
    {
        if (!_selecting) return;
        
        var mousePos = _placement.WorldMousePos;

        int x = Math.Min(_rectStart.X, mousePos.X);
        int y = Math.Min(_rectStart.Y, mousePos.Y);
        int w = Math.Abs(mousePos.X - _rectStart.X);
        int h = Math.Abs(mousePos.Y - _rectStart.Y);

        _selectionRect = new Rectangle(x, y, w, h);
    }

    public override void PressEmpty(Point worldMousePos, Point uiMousePos, bool swipe)
    {
        _selectionRect = new Rectangle();
        _selecting = true;
        _rectStart = worldMousePos;
    }

    public override void PressPrimary(LevelObject levelObject, Point worldMousePos, Point uiMousePos, bool swipe)
    {
        _selectionRect = new Rectangle();
        _selecting = true;
        _rectStart = worldMousePos;
    }

    public override void Release()
    {
        if (_selecting)
        {
            var objects = _placement.Grid.GetObjectsInRect(_selectionRect, _placement.Grid.activeLayer);
            _placement.SelectMultiple(objects);
        }

        _selecting = false;
    }

    public override void DrawWorld(SpriteBatch spriteBatch)
    {
        if (!_selecting) return;

        Debug.DrawRectangle(spriteBatch, _selectionRect, Color.LightBlue * 0.5f);
        Debug.DrawRectangleOutline(spriteBatch, _selectionRect, Color.LightBlue, 1);
    }
}