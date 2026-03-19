using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class Panel : Element
{
    Texture2D _texture;
    int _cornerSize = 16;

    public Panel(Point position, Point size, Vector2 scale, Origin origin, Anchor anchor, Texture2D texture, int cornerSize = 16) : base(position, size, scale, origin, anchor)
    {
        _texture = texture;
        _cornerSize = cornerSize;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        
        UIHelper.DrawSegmented(spriteBatch, _texture, AbsolutePosition, size, AbsoluteScale, _cornerSize, color);
    }

    public override void OnMouseHover()
    {
        Input.MouseHoverConsumed = true;
    }
}