using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Editor;
using SpringProject.Core.Scenes;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class ColorPanel : Element
{
    const int CORNER_SIZE = 3;

    Texture2D _sprite;
    Texture2D _selectedTexture;
    int _colorIndex;
    LevelEditor _levelEditor;
    
    public ColorPanel(Point localPosition, int colorIndex, LevelEditor levelEditor, Point size, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, size, anchor)
    {
        _sprite = TextureManager.Get("color_display");
        _selectedTexture = TextureManager.Get("panel_selected_small");
        _colorIndex = colorIndex;
        _levelEditor = levelEditor;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (_hovering)
        {
            Input.ConsumeHover();
        }

        UIHelper.DrawSegmented(spriteBatch, _sprite, AbsolutePosition, size, AbsoluteScale, CORNER_SIZE, ColorManager.Get(_colorIndex));

        Color outlineColor = ColorManager.SelectedColorIndex == _colorIndex ? Main.SelectedOutlineColor : Main.HoverOutlineColor;
        if (_hovering || ColorManager.SelectedColorIndex == _colorIndex)
        {
            UIHelper.DrawSegmented(spriteBatch, _selectedTexture, AbsolutePosition, size, AbsoluteScale, CORNER_SIZE, outlineColor);
        }
    }

    public override void OnPressed()
    {
        base.OnPressed();
        Input.ConsumePress();

        _levelEditor.SelectColor(_colorIndex);

        Cursor.BeginPress();
    }

    public override void OnReleased()
    {
        Cursor.EndPress();
    }

    public override void OnReleasedOff()
    {
        Cursor.EndPress();
    }

    public override void OnMouseExit()
    {
        Cursor.EndPress();
    }
}