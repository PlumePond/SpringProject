using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public static class UIHelper
{
    public static void DrawSegmented(SpriteBatch spriteBatch, Texture2D texture, Point point, Point size, Vector2 scale, int cornerSize, Color color)
    {
        Vector2 pos = point.ToVector2();
        // draw segmented panel, with corners, edges, and center
        // corners are drawn at their original size, edges are stretched in one direction, and the center is stretched in both directions
        // top left corner
        spriteBatch.Draw(
            texture, 
            pos, 
            new Rectangle(0, 0, cornerSize, cornerSize), 
            color);
        // top edge
        spriteBatch.Draw(
            texture, 
            new Rectangle(point.X + cornerSize, 
            point.Y, 
            (int)(scale.X * size.X - 2 * cornerSize), cornerSize), 
            new Rectangle(cornerSize, 0, texture.Width - 2 * cornerSize, cornerSize), 
            color);
        // top right corner
        spriteBatch.Draw(
            texture, 
            new Vector2(pos.X + scale.X * size.X - cornerSize, pos.Y), 
            new Rectangle(texture.Width - cornerSize, 0, cornerSize, cornerSize), 
            color);
        // left edge
        spriteBatch.Draw(
            texture, 
            new Rectangle(point.X, point.Y + cornerSize, cornerSize, (int)(scale.Y * size.Y - 2 * cornerSize)), 
            new Rectangle(0, cornerSize, cornerSize, texture.Height - 2 * cornerSize), 
            color);
        // center
        spriteBatch.Draw(
            texture, 
            new Rectangle(point.X + cornerSize, point.Y + cornerSize, (int)(scale.X * size.X - 2 * cornerSize), (int)(scale.Y * size.Y - 2 * cornerSize)), 
            new Rectangle(cornerSize, cornerSize, texture.Width - 2 * cornerSize, texture.Height - 2 * cornerSize), 
            color);
        // right edge
        spriteBatch.Draw(
            texture, 
            new Rectangle((int)(point.X + scale.X * size.X - cornerSize), point.Y + cornerSize, cornerSize, (int)(scale.Y * size.Y - 2 * cornerSize)), 
            new Rectangle(texture.Width - cornerSize, cornerSize, cornerSize, texture.Height - 2 * cornerSize), 
            color);
        // bottom left corner
        spriteBatch.Draw(
            texture, 
            new Vector2(pos.X, pos.Y + scale.Y * size.Y - cornerSize), 
            new Rectangle(0, texture.Height - cornerSize, cornerSize, cornerSize), 
            color);
        // bottom edge
        spriteBatch.Draw(
            texture, 
            new Rectangle(point.X + cornerSize, (int)(point.Y + scale.Y * size.Y - cornerSize), (int)(scale.X * size.X - 2 * cornerSize), cornerSize), 
            new Rectangle(cornerSize, texture.Height - cornerSize, texture.Width - 2 * cornerSize, cornerSize), 
            color);
        // bottom right corner
        spriteBatch.Draw(
            texture, 
            new Vector2(pos.X + scale.X * size.X - cornerSize, pos.Y + scale.Y * size.Y - cornerSize), 
            new Rectangle(texture.Width - cornerSize, texture.Height - cornerSize, cornerSize, cornerSize), 
            color);
    }
}