using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Debugging;
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

    public static void DrawSegmentedRepeating(SpriteBatch spriteBatch, Texture2D texture, Rectangle frame, Rectangle topLeft, Rectangle top, Rectangle topRight, Rectangle left, Rectangle mid, Rectangle right, Rectangle bottomLeft, Rectangle bottom, Rectangle bottomRight)
    {
        //Debug.DrawRectangleOutline(spriteBatch, frame, Color.LightBlue, 1);

        // top left
        Rectangle topLeftSource = new Rectangle(topLeft.X, topLeft.Y, topLeft.Width, topLeft.Height);
        Rectangle topLeftFrame = new Rectangle(frame.Left, frame.Top, topLeft.Width, topLeft.Height);
        spriteBatch.Draw(texture, topLeftFrame, topLeftSource, Color.White);

        // top
        Rectangle topSource = new Rectangle(top.X, top.Y, top.Width, top.Height);
        int topRepetitions = (frame.Width - topLeft.Width - topRight.Width) / top.Width;
        for (int i = 0; i < topRepetitions; i++)
        {
            Rectangle topFrame = new Rectangle(frame.Left + topLeft.Width + (i * top.Width), frame.Top, top.Width, top.Height);
            spriteBatch.Draw(texture, topFrame, topSource, Color.White);
        }

        // top right
        Rectangle topRightSource = new Rectangle(topRight.X, topRight.Y, topRight.Width, topRight.Height);
        Rectangle topRightFrame = new Rectangle(frame.Right - topRight.Width, frame.Top, topRight.Width, topRight.Height);
        spriteBatch.Draw(texture, topRightFrame, topRightSource, Color.White);

        // left
        Rectangle leftSource = new Rectangle(left.X, left.Y, left.Width, left.Height);
        int leftRepetitions = (frame.Height - topLeft.Height - bottomLeft.Height) / left.Height;
        for (int i = 0; i < leftRepetitions; i++)
        {
            Rectangle leftFrame = new Rectangle(frame.Left, frame.Top + topLeft.Height + (i * left.Height), left.Width, left.Height);
            spriteBatch.Draw(texture, leftFrame, leftSource, Color.White);
        }

        // mid
        Rectangle midSource = new Rectangle(mid.X, mid.Y, mid.Width, mid.Height);
        int midRepetitionsX = (frame.Width - left.Width - right.Width) / mid.Width;
        int midRepetitionsY = (frame.Height - top.Height - bottom.Height) / mid.Height;

        for (int y = 0; y < midRepetitionsY; y++)
        {
            for (int x = 0; x < midRepetitionsX; x++)
            {
                Rectangle midFrame = new Rectangle(frame.Left + left.Width + (x * mid.Width), frame.Top + top.Height + (y * mid.Height), mid.Width, mid.Height);
                spriteBatch.Draw(texture, midFrame, midSource, Color.White);
            }
        }

        // right
        Rectangle rightSource = new Rectangle(right.X, right.Y, right.Width, right.Height);
        int rightRepetitions = (frame.Height - topRight.Height - bottomRight.Height) / right.Height;
        for (int i = 0; i < rightRepetitions; i++)
        {
            Rectangle rightFrame = new Rectangle(frame.Right - right.Width, frame.Top + topRight.Height + (i * right.Height), right.Width, right.Height);
            spriteBatch.Draw(texture, rightFrame, rightSource, Color.White);
        }

        // bottom left
        Rectangle bottomLeftSource = new Rectangle(bottomLeft.X, bottomLeft.Y, bottomLeft.Width, bottomLeft.Height);
        Rectangle bottomLeftFrame = new Rectangle(frame.Left, frame.Bottom - bottomLeft.Height, bottomLeft.Width, bottomLeft.Height);
        spriteBatch.Draw(texture, bottomLeftFrame, bottomLeftSource, Color.White);

        // bottom
        Rectangle bottomSource = new Rectangle(bottom.X, bottom.Y, bottom.Width, bottom.Height);
        int bottomRepetitions = (frame.Width - bottomLeft.Width - bottomRight.Width) / bottom.Width;
        for (int i = 0; i < bottomRepetitions; i++)
        {
            Rectangle bottomFrame = new Rectangle(frame.Left + bottomLeft.Width + (i * bottom.Width), frame.Bottom - bottom.Height, bottom.Width, bottom.Height);
            spriteBatch.Draw(texture, bottomFrame, bottomSource, Color.White);
        }

        // bottom right
        Rectangle bottomRightSource = new Rectangle(bottomRight.X, bottomRight.Y, bottomRight.Width, bottomRight.Height);
        Rectangle bottomRightFrame = new Rectangle(frame.Right - bottomRight.Width, frame.Bottom - bottomRight.Height, bottomRight.Width, bottomRight.Height);
        spriteBatch.Draw(texture, bottomRightFrame, bottomRightSource, Color.White);
    }
}