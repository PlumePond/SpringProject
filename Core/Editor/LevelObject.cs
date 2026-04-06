using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringProject.Core.Editor;

public class LevelObject
{
    public LevelObjectData data { get; protected set; }
    public Transform transform { get; protected set; }
    public Color color { get; protected set; } = Color.White;
    public Color tint { get; protected set; } = Color.White;
    public Rectangle bounds { get; protected set; }
    public bool selected { get; protected set; } = false;
    public bool hovered { get; protected set; } = false;
    public Grid grid { get; protected set; } = null;
    public Point size { get; protected set; } = Point.Zero;
    public Point frame { get; protected set; } = Point.Zero;
    public int layer { get; protected set; } = 0;

    public LevelObject()
    {
    }

    public virtual void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        transform = new Transform();
        this.data = data;
        this.grid = grid;
        transform.position = position;
        size = data.size;
        frame = data.frame;
        CalculateBounds();
    }

    public virtual void OnPlaced()
    {
        
    }

    public virtual void OnRemoved()
    {
        
    }

    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void EditorUpdate(GameTime gameTime)
    {
        
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        Point framedSize = data.frame != Point.Zero ? data.frame : data.size;
        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Vector2 origin = new Vector2(framedSize.X / 2f, framedSize.Y / 2f);
        float radians = transform.rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (transform.flipX) effects |= SpriteEffects.FlipHorizontally;
        if (transform.flipY) effects |= SpriteEffects.FlipVertically;

        Vector2 drawScale = new Vector2((float)size.X / data.sprite.Width, (float)size.Y / data.sprite.Height);

        Color objectColor = selected ? Color.LightGoldenrodYellow * color : color;
        
        Rectangle? sourceRect = frame != Point.Zero ? new Rectangle(data.defaultFramePos, frame) : null;
        Rectangle? outlineSourceRect = frame != Point.Zero ? new Rectangle(data.frameOutline ? data.defaultFramePos : Point.Zero, frame) : null;

        spriteBatch.Draw(data.sprite, drawPos, sourceRect, objectColor * tint, radians, origin, drawScale, effects, 0f);

        if (hovered)
        {
            spriteBatch.Draw(data.outline, drawPos, outlineSourceRect, Color.White, radians, origin, drawScale, effects, 0f);
        }
        else if (selected)
        {
            spriteBatch.Draw(data.outline, drawPos, outlineSourceRect, Color.Yellow, radians, origin, drawScale, effects, 0f);
        }
    }

    public virtual void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        Color hitboxColor = data.solid ? Color.Green : Color.Blue;
        Debug.DrawRectangle(spriteBatch, bounds, hitboxColor * 0.25f);

        string debugText = $"{data.material}";
        Vector2 textPos = bounds.Center.ToVector2();
        Vector2 textOrigin = font.FontBase.MeasureString(debugText) * 0.5f;
        spriteBatch.DrawString(font.FontBase, debugText, textPos, Color.White, 0, textOrigin, Vector2.One * 0.25f);
        
        if (hovered)
        {
            Debug.DrawRectangleOutline(spriteBatch, bounds, Color.White, 1);   
        }
        else if (selected)
        {
            Debug.DrawRectangleOutline(spriteBatch, bounds, Color.Yellow, 1);   
        }
        else
        {
            Debug.DrawRectangleOutline(spriteBatch, bounds, hitboxColor, 1);   
        }
    }

    public void SetPosition(Point position)
    {
        transform.position = position;
        CalculateBounds();
    }

    public void SetSize(Point size)
    {
        this.size = size;
        CalculateBounds();
    }

    public void SetFlipX(bool flipX)
    {
        transform.flipX = flipX;
    }

    public void SetFlipY(bool flipY)
    {
        transform.flipY = flipY;
    }

    public void RotateClockwise()
    {
        transform.rotation = (transform.rotation + 90) % 360;
        CalculateBounds();
    }

    public void RotateCounterClockwise()
    {
        transform.rotation = (transform.rotation + 270) % 360;
        CalculateBounds();
    }
    
    public void SetRotation(int rotation)
    {
        transform.rotation = rotation % 360;
        CalculateBounds();
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    public void SetLayer(int layer)
    {
        this.layer = layer;
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
    }

    public void SetHovered(bool hovered)
    {
        this.hovered = hovered;
    }

    public void SetTint(Color tint)
    {
        this.tint = tint;
    }

    public virtual void CalculateBounds()
    {
        Point framedSize = frame != Point.Zero ? frame : size;
        bool swapDimensions = transform.rotation == 90 || transform.rotation == 270;
        Point rotatedSize = swapDimensions ? new Point(framedSize.Y, framedSize.X) : new Point(framedSize.X, framedSize.Y);  
        bounds = new Rectangle(transform.position.X, transform.position.Y, rotatedSize.X, rotatedSize.Y);
    }
}