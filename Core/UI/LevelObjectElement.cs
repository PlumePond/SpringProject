using System;
using System.ComponentModel.Design;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Audio;
using SpringProject.Core.Editor;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class LevelObjectElement : Element
{
    public LevelObjectData LevelObjectData => _levelObjectData;

    LevelObjectData _levelObjectData;
    GridPlacement _gridPlacement;
    Texture2D _texture;
    bool _hovering = false;

    public LevelObjectElement(Point position, Anchor anchor, Point size, LevelObjectData levelObjectData, GridPlacement gridPlacement) : base(position, size, anchor)
    {
        _levelObjectData = levelObjectData;
        _texture = levelObjectData.sprite;
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

        Point frame = _levelObjectData.frame != Point.Zero ? _levelObjectData.frame : _levelObjectData.size;
        // Point frame = new Point(16);

        if (frame.X > 16)
        {
            frame.X = (int)MathF.Floor(frame.X / 16) * 16;
        }
        if (frame.Y > 16)
        {
            frame.Y = (int)MathF.Floor(frame.Y / 16) * 16;
        }

        Rectangle destRect = new Rectangle(AbsolutePosition, frame * AbsoluteScale.ToPoint());
        Rectangle sourceRect = new Rectangle(_levelObjectData.defaultFramePos, frame);

        // to prevent the frame pos from affecting the outline
        Rectangle outlineSourceRect = new Rectangle(_levelObjectData.frameOutline ? _levelObjectData.defaultFramePos : Point.Zero, frame);

        // draw in a rectangle that is defined by the position, size, and scale of the element
        spriteBatch.Draw(_texture, destRect, sourceRect, color);

        if (_hovering)
        {
            spriteBatch.Draw(_levelObjectData.outline, destRect, outlineSourceRect, Color.White);
        }
        else if (_gridPlacement.SelectedObjectData == _levelObjectData)
        {
            spriteBatch.Draw(_levelObjectData.outline, destRect, outlineSourceRect, Main.UIEnabledColor);
        }
    }

    public override bool WithinBounds(Point point)
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

        Rectangle rect = new Rectangle(AbsolutePosition, frame * AbsoluteScale.ToPoint());

        return rect.Contains(point);
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
        }

        Cursor.BeginPress();
    }

    public override void OnReleased()
    {
        SetColor(Color.White);
        Cursor.EndPress();
    }
}