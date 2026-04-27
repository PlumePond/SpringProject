using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Components;

public class BoxCollider : Collider
{
    public override bool ResolveX(ref Vector2 position, ref Vector2 internalVelocity, ref Vector2 externalVelocity, Rectangle nextHitbox, Point hitboxOffset)
    {
        float velocityX = internalVelocity.X + externalVelocity.X;

        if (velocityX > 0 && Bounds.Left >= nextHitbox.Left)
        {
            position.X = Bounds.Left - nextHitbox.Width - hitboxOffset.X;
            return true;
        }
        else if (velocityX < 0 && Bounds.Right <= nextHitbox.Right)
        {
            position.X = Bounds.Right - hitboxOffset.X;
            return true;
        }

        return false;
    }

    public override bool ResolveY(ref Vector2 position, ref Vector2 internalVelocity, ref Vector2 externalVelocity, Rectangle nextHitbox, Point hitboxOffset)
    {
        float velocityY = internalVelocity.Y + externalVelocity.Y;
        
        if (velocityY > 0 && Bounds.Top >= nextHitbox.Top)
        {
            position.Y = Bounds.Top - nextHitbox.Height - hitboxOffset.Y;
            return true;
        }
        else if (velocityY < 0 && Bounds.Bottom <= nextHitbox.Bottom)
        {
            position.Y = Bounds.Bottom - hitboxOffset.Y;
            return true;
        }

        return false;
    }

    public override void DrawDebug(SpriteBatch spriteBatch)
    {
        Color hitboxColor = LevelObject.data.solid ? Color.Green : Color.Blue;
        Debug.DrawRectangle(spriteBatch, Bounds, hitboxColor * 0.25f);
        
        if (LevelObject.hovered)
        {
            Debug.DrawRectangleOutline(spriteBatch, Bounds, Color.White, 1);   
        }
        else if (LevelObject.selected)
        {
            Debug.DrawRectangleOutline(spriteBatch, Bounds, Color.Yellow, 1);   
        }
        else
        {
            Debug.DrawRectangleOutline(spriteBatch, Bounds, hitboxColor, 1);   
        }
    }
}