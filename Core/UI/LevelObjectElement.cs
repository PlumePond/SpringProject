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

namespace SpringProject.Core.UI;

public class LevelObjectElement : Element
{
    public LevelObjectData LevelObjectData => _levelObjectData;

    LevelObjectData _levelObjectData;
    Texture2D _texture;
    bool _hovering = false;
    
    Color _outlineColor = Color.White;

    public LevelObjectElement(Point position, Vector2 scale, Point size, LevelObjectData levelObjectData) : base(position, size, scale, Origin.MiddleCenter, Anchor.MiddleCenter)
    {
        _levelObjectData = levelObjectData;
        _texture = levelObjectData.sprite;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (Main.Grid.SelectedObjectData == _levelObjectData)
        {
            color = Color.LightGoldenrodYellow;
        }
        else
        {
            color = Color.White;
        }

        // draw in a rectangle that is defined by the position, size, and scale of the element
        spriteBatch.Draw(_texture, new Rectangle(AbsolutePosition, size * AbsoluteScale.ToPoint()), color);

        if (_hovering)
        {
            spriteBatch.Draw(_levelObjectData.outline, new Rectangle(AbsolutePosition, size * AbsoluteScale.ToPoint()), Color.White);
        }
        else if (Main.Grid.SelectedObjectData == _levelObjectData)
        {
            spriteBatch.Draw(_levelObjectData.outline, new Rectangle(AbsolutePosition, size * AbsoluteScale.ToPoint()), Color.Yellow);
        }
    }

    public override void OnMouseEnter()
    {
        _hovering = true;
        Main.MouseHoverConsumed = true;
    }

    public override void OnMouseHover()
    {
        Main.MouseHoverConsumed = true;
    }

    public override void OnMouseExit()
    {
        _outlineColor = Color.White;
        _hovering = false;
    }

    public override void OnPressed()
    {
        Main.MousePressConsumed = true;
        
        _outlineColor = Color.Yellow;
        AudioManager.Get("accept").Play();

        if (Main.Grid.SelectedObjectData == _levelObjectData)
        {
            Main.Grid.SetSelectedObjectData(null);
        }
        else
        {
            Main.Grid.SetSelectedObjectData(_levelObjectData);
        }
    }

    public override void OnReleased()
    {
        SetColor(Color.White);
    }
}