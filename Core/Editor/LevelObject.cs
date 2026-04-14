using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NVorbis;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using StbImageSharp;
using System;

namespace SpringProject.Core.Editor;

public class LevelObject
{
    public LevelObjectData data { get; protected set; }
    public Transform transform { get; protected set; }
    public Color tint { get; protected set; } = Color.White;
    public int colorIndex { get; protected set; } = 0;
    public Rectangle bounds { get; protected set; }
    public Rectangle hitbox { get; protected set; }
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
        Color objectColor = selected ? Color.LightGoldenrodYellow * ColorManager.Get(colorIndex) : ColorManager.Get(colorIndex);
        Rectangle? sourceRect = frame != Point.Zero ? new Rectangle(data.defaultFramePos, frame) : null;

        spriteBatch.Draw(data.sprite, drawPos, sourceRect, objectColor * tint, radians, origin, drawScale, effects, 0);
    }

    public virtual void DrawOutline(SpriteBatch spriteBatch)
    {
        // no need to draw this if it is not selected or hovered boii
        if (!hovered && !selected) return;

        Point framedSize = data.frame != Point.Zero ? data.frame : data.size;
        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Vector2 origin = new Vector2(framedSize.X / 2f, framedSize.Y / 2f);
        float radians = transform.rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (transform.flipX) effects |= SpriteEffects.FlipHorizontally;
        if (transform.flipY) effects |= SpriteEffects.FlipVertically;

        Vector2 drawScale = new Vector2((float)size.X / data.sprite.Width, (float)size.Y / data.sprite.Height);
        Rectangle? sourceRect = frame != Point.Zero ? new Rectangle(data.frameOutline ? data.defaultFramePos : Point.Zero, frame) : null;

        Color outlineColor = selected ? Main.SelectedOutlineColor : Main.HoverOutlineColor;
        TextureUtils.DrawOutlineExpanded(spriteBatch, data.alphaTexture, drawPos, sourceRect, outlineColor, radians, origin, drawScale, effects, 0);
    }

    public virtual void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        Color hitboxColor = data.solid ? Color.Green : Color.Blue;
        Debug.DrawRectangle(spriteBatch, hitbox, hitboxColor * 0.25f);

        string debugText = $"{data.material}";
        Vector2 textPos = hitbox.Center.ToVector2();
        Vector2 textOrigin = font.FontBase.MeasureString(debugText) * 0.5f;
        spriteBatch.DrawString(font.FontBase, debugText, textPos, Color.White, 0, textOrigin, Vector2.One * 0.5f);
        
        if (hovered)
        {
            Debug.DrawRectangleOutline(spriteBatch, hitbox, Color.White, 1);   
        }
        else if (selected)
        {
            Debug.DrawRectangleOutline(spriteBatch, hitbox, Color.Yellow, 1);   
        }
        else
        {
            Debug.DrawRectangleOutline(spriteBatch, hitbox, hitboxColor, 1);   
        }
    }

    public virtual void SetPosition(Point position)
    {
        transform.position = position;
        CalculateBounds();
        UpdateInfo();
    }

    public void SetSize(Point size)
    {
        this.size = size;
        CalculateBounds();
    }

    public void SetFlipX(bool flipX)
    {
        transform.flipX = flipX;
        CalculateHitbox();
        UpdateInfo();
    }

    public void SetFlipY(bool flipY)
    {
        transform.flipY = flipY;
        CalculateHitbox();
        UpdateInfo();
    }

    public void RotateClockwise()
    {
        transform.rotation = (transform.rotation + 90) % 360;
        CalculateBounds();
        UpdateInfo();
    }

    public void RotateCounterClockwise()
    {
        transform.rotation = (transform.rotation + 270) % 360;
        CalculateBounds();
        UpdateInfo();
    }
    
    public void SetRotation(int rotation)
    {
        transform.rotation = rotation % 360;
        CalculateBounds();
        UpdateInfo();
    }

    public void SetColorIndex(int index)
    {
        this.colorIndex = index;
    }

    public void SetTint(Color tint)
    {
        this.tint = tint;
    }

    public void SetLayer(int layer)
    {
        this.layer = layer;
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;

        if (selected)
        {
            SetInfo();
        }
    }

    public void SetHovered(bool hovered)
    {
        this.hovered = hovered;
    }

    public virtual void CalculateBounds()
    {
        Point framedSize = frame != Point.Zero ? frame : size;
        bool swapDimensions = transform.rotation == 90 || transform.rotation == 270;
        Point rotatedSize = swapDimensions ? new Point(framedSize.Y, framedSize.X) : new Point(framedSize.X, framedSize.Y);  
        bounds = new Rectangle(transform.position.X, transform.position.Y, rotatedSize.X, rotatedSize.Y);

        CalculateHitbox();
    }

    protected virtual void CalculateHitbox()
    {
        Rectangle tempHitbox = Rectangle.Empty;
        Point framedSize = frame != Point.Zero ? frame : size;

        if (data.hitbox.Equals(Rectangle.Empty))
        {
            bool swapDimensions = transform.rotation == 90 || transform.rotation == 270;
            Point rotatedSize = swapDimensions ? new Point(framedSize.Y, framedSize.X) : new Point(framedSize.X, framedSize.Y);
            tempHitbox = new Rectangle(transform.position.X, transform.position.Y, rotatedSize.X, rotatedSize.Y);
        }
        else
        {
            int offsetX = transform.flipX ? framedSize.X - data.hitbox.Location.X - data.hitbox.Width : data.hitbox.Location.X;
            int offsetY = transform.flipY ? framedSize.Y - data.hitbox.Location.Y - data.hitbox.Height : data.hitbox.Location.Y;
            var offset = new Point(offsetX, offsetY);

            tempHitbox = new Rectangle(transform.position + offset, data.hitbox.Size);
        }

        hitbox = tempHitbox;
    }

    public virtual void SetInfo()
    {
        InfoPanel.ClearElements();

        InfoPanel.AddElement("name", new TextElement(new Point(4, 3), FontManager.Get("body"), data.name, Main.SelectedOutlineColor, Anchor.TopLeft));
        InfoPanel.AddElement("pos", new TextElement(new Point(4, 13), FontManager.Get("body"), $"pos: ({transform.position.X}, {transform.position.Y})", Color.White, Anchor.TopLeft));
        InfoPanel.AddElement("color", new TextElement(new Point(4, 23), FontManager.Get("body"), $"color: {colorIndex}", ColorManager.Get(colorIndex), Anchor.TopLeft));
    }

    public virtual void UpdateInfo()
    {
        if (InfoPanel.TryGetElement<TextElement>("pos", out var text))
        {
            text.SetText($"pos: ({transform.position.X}, {transform.position.Y})");
        }
    }
}