using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class Panel : Element
{
    protected int _cornerSize = 16;
    protected string _textureName = "";

    public Panel(Point position, Point size, Anchor anchor, string textureName, int cornerSize = 3) : base(position, size, anchor)
    {
        _cornerSize = cornerSize;
        _textureName = textureName;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        UIHelper.DrawSegmented(spriteBatch, TextureManager.Get(_textureName), AbsolutePosition, size, AbsoluteScale, _cornerSize, color);
        
        base.Draw(spriteBatch);
    }

    public override void OnMouseHover()
    {
        Input.ConsumeHover();
    }
}