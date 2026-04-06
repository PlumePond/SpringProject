using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Debugging;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class Panel : Element
{
    Texture2D _texture;
    int _cornerSize = 16;

    public Panel(Point position, Point size, Anchor anchor, Texture2D texture, int cornerSize = 3) : base(position, size, anchor)
    {
        _texture = texture;
        _cornerSize = cornerSize;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        UIHelper.DrawSegmented(spriteBatch, _texture, AbsolutePosition, size, AbsoluteScale, _cornerSize, color);
        
        base.Draw(spriteBatch);
    }

    public override void OnMouseHover()
    {
        Input.ConsumeHover();
    }
}