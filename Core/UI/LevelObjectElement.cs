using System;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Audio;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class LevelObjectElement : Element
{
    public LevelObjectData LevelObjectData => _levelObjectData;

    LevelObjectData _levelObjectData;
    GridPlacement _gridPlacement;
    bool _hovering = false;

    const int MAX_WIDTH = 3;
    const int MAX_HEIGHT = 3;

    public LevelObjectElement(Point position, Anchor anchor, Point size, LevelObjectData levelObjectData, GridPlacement gridPlacement) : base(position, size, anchor)
    {
        _levelObjectData = levelObjectData;
        _gridPlacement = gridPlacement;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (_gridPlacement.SelectedObjectData == _levelObjectData)
        {
            color = ColorUtils.Desaturate(Main.UIEnabledColor, 0.6f);
        }
        else
        {
            color = Color.White;
        }

        Rectangle destRect = new Rectangle(AbsolutePosition, GetFrame() * AbsoluteScale.ToPoint());
        Rectangle sourceRect = new Rectangle(_levelObjectData.defaultFramePos, GetFrame());

        if (_gridPlacement.SelectedObjectData == _levelObjectData || _hovering)
        {
            var outlineColor = (_gridPlacement.SelectedObjectData == _levelObjectData) ? Main.SelectedOutlineColor : Main.HoverOutlineColor;
            TextureUtils.DrawOutlineExpanded(spriteBatch, _levelObjectData.alphaTexture, sourceRect, destRect, outlineColor);
        }

        // draw in a rectangle that is defined by the position, size, and scale of the element
        spriteBatch.Draw(_levelObjectData.sprite, destRect, sourceRect, color);
    }

    public override bool WithinBounds(Point point)
    {
        Rectangle rect = new Rectangle(AbsolutePosition, GetFrame());

        return rect.Contains(point);
    }

    Point GetFrame()
    {
        Point frame = _levelObjectData.frame != Point.Zero ? _levelObjectData.frame : _levelObjectData.size;

        if (frame.X > 16)
        {
            frame.X = (int)MathF.Floor(frame.X / 16) * 16;
        }
        if (frame.Y > 16)
        {
            frame.Y = (int)MathF.Floor(frame.Y / 16) * 16;
        }

        frame.X = Math.Min(frame.X, MAX_WIDTH * 16);
        frame.Y = Math.Min(frame.Y, MAX_HEIGHT * 16);

        return frame * AbsoluteScale.ToPoint();
    }
    

    public override void OnMouseEnter()
    {
        _hovering = true;

        Cursor.BeginHover();
    }

    public override void OnMouseHover()
    {
        Input.ConsumeHover();
    }

    public override void OnMouseExit()
    {
        _hovering = false;

        Cursor.EndHover();
    }

    public override void OnPressed()
    {
        Input.ConsumePress();
        
        AudioManager.Get("accept").Play();

        if (_gridPlacement.SelectedObjectData == _levelObjectData)
        {
            _gridPlacement.SetSelectedObjectData(null);
        }
        else
        {
            _gridPlacement.SetSelectedObjectData(_levelObjectData);
            SetInfo();
        }

        Cursor.BeginPress();
    }

    public override void OnReleased()
    {
        SetColor(Color.White);
        Cursor.EndPress();
    }

    public void SetInfo()
    {
        InfoPanel.ClearElements();

        InfoPanel.AddElement("text", new TextElement(new Point(4, 3), FontManager.Get("body"), _levelObjectData.name, Main.SelectedOutlineColor, Anchor.TopLeft));
        InfoPanel.AddElement("type", new TextElement(new Point(4, 14), FontManager.Get("body"), $"type: {_levelObjectData.typeName}", Color.White, Anchor.TopLeft));
    }
}