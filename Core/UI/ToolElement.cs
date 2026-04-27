using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Editor;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class ToolElement : Element
{
    string _inactiveTexture;
    string _activeTexture;
    string _selectedTexture;
    string _displayTexture;

    int _cornerSize = 16;
    bool _selected = false;
    bool _pressed = false;
    Tool _tool;

    ImageElement _displayElement;

    bool _active = false;

    public Action<bool> ValueChanged;

    public ToolElement(Point position, Point size, Anchor anchor, string inactiveTeture, string activeTexture, string selectedTexture, string displayTexture, Tool tool, int cornerSize = 3) : base(position, size, anchor)
    {
        _inactiveTexture = inactiveTeture;
        _activeTexture = activeTexture;
        _selectedTexture = selectedTexture;
        _displayTexture = displayTexture;

        _cornerSize = cornerSize;
        _tool = tool;

        _displayElement = new ImageElement(Point.Zero, Anchor.MiddleCenter, _displayTexture, Main.UIDefaultColor);
        AddChild(_displayElement);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _active = _tool == GridPlacement.CurrentTool;
        UIHelper.DrawSegmented(spriteBatch, _active ? TextureManager.Get(_activeTexture) : TextureManager.Get(_inactiveTexture), AbsolutePosition, size, AbsoluteScale, _cornerSize, color);
        if (_selected && !_pressed)
        {
            UIHelper.DrawSegmented(spriteBatch, TextureManager.Get(_selectedTexture), AbsolutePosition, size, AbsoluteScale, _cornerSize, color);
        }

        _displayElement.SetColor(_active ? Main.UIEnabledColor : Main.UIDefaultColor);

        base.Draw(spriteBatch);
    }

    public override void OnMouseEnter()
    {
        _selected = true;
        Cursor.BeginHover();
    }

    public override void OnMouseHover()
    {
        Input.ConsumeHover();
    }

    public override void OnMouseExit()
    {
        _selected = false;
        _pressed = false;
        Cursor.EndPress();
        Cursor.EndHover();
    }

    public override void OnPressed()
    {
        _pressed = true;
        Input.ConsumePress();
        Cursor.BeginPress();
    }

    public override void OnReleased()
    {
        _pressed = false;

        Cursor.EndPress();
        Set();
    }

    public void Set()
    {
        GridPlacement.SetTool(_tool);
    }
}