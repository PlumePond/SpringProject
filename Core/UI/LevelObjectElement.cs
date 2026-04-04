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
    
    Color _outlineColor = Color.White;

    public LevelObjectElement(Point position, Vector2 scale, Point size, LevelObjectData levelObjectData, GridPlacement gridPlacement) : base(position, size, scale, Origin.MiddleCenter, Anchor.MiddleCenter)
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
            color = Color.LightGoldenrodYellow;
        }
        else
        {
            color = Color.White;
        }

        Point frame = _levelObjectData.frame != Point.Zero ? _levelObjectData.frame : _levelObjectData.size;

        Rectangle destRect = new Rectangle(AbsolutePosition, size * AbsoluteScale.ToPoint());
        Rectangle sourceRect = new Rectangle(_levelObjectData.defaultFramePos, frame);

        // draw in a rectangle that is defined by the position, size, and scale of the element
        spriteBatch.Draw(_texture, destRect, sourceRect, color);

        if (_hovering)
        {
            spriteBatch.Draw(_levelObjectData.outline, destRect, Color.White);
        }
        else if (_gridPlacement.SelectedObjectData == _levelObjectData)
        {
            spriteBatch.Draw(_levelObjectData.outline, destRect, Color.Yellow);
        }
    }

    public override void OnMouseEnter()
    {
        _hovering = true;
    }

    public override void OnMouseHover()
    {
        Input.ConsumeHover();
    }

    public override void OnMouseExit()
    {
        _outlineColor = Color.White;
        _hovering = false;
    }

    public override void OnPressed()
    {
        Input.ConsumePress();
        
        _outlineColor = Color.Yellow;
        AudioManager.Get("accept").Play();

        if (_gridPlacement.SelectedObjectData == _levelObjectData)
        {
            _gridPlacement.SetSelectedObjectData(null);
        }
        else
        {
            _gridPlacement.SetSelectedObjectData(_levelObjectData);
        }
    }

    public override void OnReleased()
    {
        SetColor(Color.White);
    }
}