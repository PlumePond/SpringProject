using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        // if (size.X == 100)
        // {
        //     Environment.Exit(0);
        // }
        
        // draw segmented panel, with corners, edges, and center
        // corners are drawn at their original size, edges are stretched in one direction, and the center is stretched in both directions

        Point offset = Point.Zero; // can be used to add padding if desired

        // top left corner
        spriteBatch.Draw(
            _texture, 
            (AbsolutePosition + offset).ToVector2(), 
            new Rectangle(0, 0, _cornerSize, _cornerSize), 
            color);
        // top edge
        spriteBatch.Draw(
            _texture, 
            new Rectangle(AbsolutePosition.X + offset.X + _cornerSize, 
            AbsolutePosition.Y + offset.Y, 
            (int)(AbsoluteScale.X * size.X - 2 * _cornerSize), _cornerSize), 
            new Rectangle(_cornerSize, 0, _texture.Width - 2 * _cornerSize, _cornerSize), 
            color);
        // top right corner
        spriteBatch.Draw(
            _texture, 
            new Vector2(AbsolutePosition.X + AbsoluteScale.X * size.X - _cornerSize + offset.X, AbsolutePosition.Y + offset.Y), 
            new Rectangle(_texture.Width - _cornerSize, 0, _cornerSize, _cornerSize), 
            color);
        // left edge
        spriteBatch.Draw(
            _texture, 
            new Rectangle(AbsolutePosition.X + offset.X, AbsolutePosition.Y + offset.Y + _cornerSize, _cornerSize, (int)(AbsoluteScale.Y * size.Y - 2 * _cornerSize)), 
            new Rectangle(0, _cornerSize, _cornerSize, _texture.Height - 2 * _cornerSize), 
            color);
        // center
        spriteBatch.Draw(
            _texture, 
            new Rectangle(AbsolutePosition.X + offset.X + _cornerSize, AbsolutePosition.Y + offset.Y + _cornerSize, (int)(AbsoluteScale.X * size.X - 2 * _cornerSize), (int)(AbsoluteScale.Y * size.Y - 2 * _cornerSize)), 
            new Rectangle(_cornerSize, _cornerSize, _texture.Width - 2 * _cornerSize, _texture.Height - 2 * _cornerSize), 
            color);
        // right edge
        spriteBatch.Draw(
            _texture, 
            new Rectangle((int)(AbsolutePosition.X + offset.X + AbsoluteScale.X * size.X - _cornerSize), AbsolutePosition.Y + offset.Y + _cornerSize, _cornerSize, (int)(AbsoluteScale.Y * size.Y - 2 * _cornerSize)), 
            new Rectangle(_texture.Width - _cornerSize, _cornerSize, _cornerSize, _texture.Height - 2 * _cornerSize), 
            color);
        // bottom left corner
        spriteBatch.Draw(
            _texture, 
            new Vector2(AbsolutePosition.X + offset.X, AbsolutePosition.Y + offset.Y + AbsoluteScale.Y * size.Y - _cornerSize), 
            new Rectangle(0, _texture.Height - _cornerSize, _cornerSize, _cornerSize), 
            color);
        // bottom edge
        spriteBatch.Draw(
            _texture, 
            new Rectangle(AbsolutePosition.X + offset.X + _cornerSize, (int)(AbsolutePosition.Y + offset.Y + AbsoluteScale.Y * size.Y - _cornerSize), (int)(AbsoluteScale.X * size.X - 2 * _cornerSize), _cornerSize), 
            new Rectangle(_cornerSize, _texture.Height - _cornerSize, _texture.Width - 2 * _cornerSize, _cornerSize), 
            color);
        // bottom right corner
        spriteBatch.Draw(
            _texture, 
            new Vector2(AbsolutePosition.X + offset.X + AbsoluteScale.X * size.X - _cornerSize, AbsolutePosition.Y + offset.Y + AbsoluteScale.Y * size.Y - _cornerSize), 
            new Rectangle(_texture.Width - _cornerSize, _texture.Height - _cornerSize, _cornerSize, _cornerSize), 
            color);
    }

    public override void OnMouseHover()
    {
        Main.MouseHoverConsumed = true;
    }
}