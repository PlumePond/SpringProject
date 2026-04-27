using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Components;

public enum SlopeDirection { RisingLeft, RisingRight }

public class SlopeCollider : Collider
{
    public SlopeDirection Direction = SlopeDirection.RisingLeft;

    private const float EdgeBiasBase = 1f;
    private const float EdgeBiasVelocityScale = 0.5f;

    private float GetEdgeBias(float velocityX, float velocityY)
    {
        float speed = MathF.Sqrt(velocityX * velocityX + velocityY * velocityY);
        return -(EdgeBiasBase + speed * EdgeBiasVelocityScale);
    }

    public float GetSurfaceX(int worldY)
    {
        float t = (float)(worldY - Bounds.Top) / Bounds.Height;
        return Direction == SlopeDirection.RisingRight ? Bounds.Left + t * Bounds.Width : Bounds.Right - t * Bounds.Width;
    }

    public float GetSurfaceY(int worldX)
    {
        float t = (float)(worldX - Bounds.Left) / Bounds.Width;
        return Direction == SlopeDirection.RisingRight ? Bounds.Bottom - t * Bounds.Height : Bounds.Top + t * Bounds.Height;
    }

    public override bool ResolveX(ref Vector2 position, ref Vector2 internalVelocity, ref Vector2 externalVelocity, Rectangle nextHitbox, Point hitboxOffset)
    {
        float velocityX = internalVelocity.X + externalVelocity.X;
        float velocityY = internalVelocity.Y + externalVelocity.Y;

        float edgeBias = GetEdgeBias(velocityX, velocityY);

        if (Direction == SlopeDirection.RisingLeft)
        {
            if (velocityX > 0 && Bounds.Left >= nextHitbox.Left)
            {
                position.X = Bounds.Left - nextHitbox.Width - hitboxOffset.X + edgeBias;
                return true;
            }
        }
        else if (Direction == SlopeDirection.RisingRight)
        {
            if (velocityX < 0 && Bounds.Right <= nextHitbox.Right)
            {
                position.X = Bounds.Right - hitboxOffset.X + edgeBias;
                return true;
            }
        }

        return false;
    }

    public override bool ResolveY(ref Vector2 position, ref Vector2 internalVelocity, ref Vector2 externalVelocity, Rectangle nextHitbox, Point hitboxOffset)
    {
        float velocityX = internalVelocity.X + externalVelocity.X;
        float velocityY = internalVelocity.Y + externalVelocity.Y;

        float edgeBias = GetEdgeBias(velocityX, velocityY);

        if (velocityY > 0)
        {
            if (Direction == SlopeDirection.RisingLeft)
            {
                float surfaceY = GetSurfaceY(nextHitbox.Left);

                if (nextHitbox.Left > Bounds.Left && nextHitbox.Bottom >= surfaceY + edgeBias)
                {
                    position.Y = surfaceY - nextHitbox.Height - hitboxOffset.Y + edgeBias;
                    return true;
                }
                else if (nextHitbox.Left <= Bounds.Left && Bounds.Top >= nextHitbox.Top)
                {
                    position.Y = Bounds.Top - nextHitbox.Height - hitboxOffset.Y;
                    return true;
                }
            }
            else if (Direction == SlopeDirection.RisingRight)
            {
                float surfaceY = GetSurfaceY(nextHitbox.Right);

                if (nextHitbox.Right < Bounds.Right && nextHitbox.Bottom >= surfaceY + edgeBias)
                {
                    position.Y = surfaceY - nextHitbox.Height - hitboxOffset.Y + edgeBias;
                    return true;
                }
                else if (nextHitbox.Right >= Bounds.Right && Bounds.Top >= nextHitbox.Top)
                {
                    position.Y = Bounds.Top - nextHitbox.Height - hitboxOffset.Y;
                    return true;
                }
            }
        }
        else if (velocityY < 0 && Bounds.Bottom <= nextHitbox.Bottom)
        {
            position.Y = Bounds.Bottom - hitboxOffset.Y;
            return true;
        }

        return false;
    }

    public override bool Overlaps(Rectangle movingHitbox)
    {
        if (!movingHitbox.Intersects(Bounds)) return false;

        float surfaceY = Direction == SlopeDirection.RisingLeft
            ? GetSurfaceY(movingHitbox.Left)
            : GetSurfaceY(movingHitbox.Right);

        return movingHitbox.Bottom >= surfaceY;
    }

    public override void DrawDebug(SpriteBatch spriteBatch)
    {
        Color hitboxColor = LevelObject.data.solid ? Color.Green : Color.Blue;

        var topLeft = new Point(Bounds.Left, Bounds.Top);
        var topRight = new Point(Bounds.Right, Bounds.Top);
        var bottomLeft = new Point(Bounds.Left, Bounds.Bottom);
        var bottomRight = new Point(Bounds.Right, Bounds.Bottom);

        if (Direction == SlopeDirection.RisingLeft)
        {
            Debug.DrawLine(spriteBatch, bottomLeft, bottomRight, hitboxColor); // opposite
            Debug.DrawLine(spriteBatch, topLeft, bottomLeft, hitboxColor); // adjacent
            Debug.DrawLine(spriteBatch, topLeft, bottomRight, hitboxColor); // hypotenuse
        }
        else if (Direction == SlopeDirection.RisingRight)
        {
            Debug.DrawLine(spriteBatch, bottomLeft, bottomRight, hitboxColor); // opposite
            Debug.DrawLine(spriteBatch, topRight, bottomRight, hitboxColor); // adjacent
            Debug.DrawLine(spriteBatch, topRight, bottomLeft, hitboxColor); // hypotenuse
        }
    }
}