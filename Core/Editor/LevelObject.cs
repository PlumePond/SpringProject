using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NVorbis;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using StbImageSharp;
using System;
using System.Collections.Generic;
using SpringProject.Core.Components;
using System.Linq;

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

    public List<Component> Components { get; private set; } = new List<Component>();

    public T AddComponent<T>() where T : Component, new()
    {
        var component = new T();
        Components.Add(component);
        component.LevelObject = this;
        component.Start();
        return component;
    }

    public void RemoveComponent<T>() where T : Component
    {
        var component = Components.OfType<T>().FirstOrDefault();
        if (component == null) return;

        component.OnDestroy();
        ComponentSystem.Remove(component);
        Components.Remove(component);
    }

    public T GetComponent<T>() where T : Component
    {
        return Components.OfType<T>().FirstOrDefault();
    }

    public virtual float ResizeDistance => 4;

    public LevelObject()
    {
    }

    public virtual void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        transform = AddComponent<Transform>();
        this.data = data;
        this.grid = grid;
        transform.position = position;
        size = data.size;
        frame = data.frame;
        CalculateBounds();
        AddComponent<Sprite>();
    }

    public virtual void OnPlaced()
    {
        
    }

    public virtual void OnRemoved()
    {
        foreach (var component in Components)
        {
            component.OnDestroy();
        }
    }

    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void FixedUpdate(GameTime gameTime)
    {
        
    }

    public virtual void EditorUpdate(GameTime gameTime)
    {
        
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        foreach (var component in Components)
        {
            component.Draw(spriteBatch);
        }
    }

    public virtual void DrawEditor(SpriteBatch spriteBatch)
    {
        foreach (var component in Components)
        {
            component.Draw(spriteBatch);
        }
    }

    public virtual void DrawOutline(SpriteBatch spriteBatch)
    {
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

        foreach (var component in Components)
        {
            component.DrawDebug(spriteBatch);
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

        // InfoPanel.AddElement("name", new TextElement(Point.Zero, FontManager.Get("body"), data.name, Main.SelectedOutlineColor, Anchor.TopLeft));
        // InfoPanel.AddElement("pos", new TextElement(Point.Zero, FontManager.Get("body"), $"pos: ({transform.position.X}, {transform.position.Y})", Color.White, Anchor.TopLeft));
        // InfoPanel.AddElement("color", new TextElement(Point.Zero, FontManager.Get("body"), $"color: {colorIndex}", ColorManager.Get(colorIndex), Anchor.TopLeft));

        // scan self and all components for parameters
        var targets = new List<object> { this };
        targets.AddRange(Components);

        var sliderTexture = "panel_dark";
        var panelTexture = "panel_light_gold";
        var selectedTexture = "panel_selected";
        var fillTexture = "slider_fill";

        var sliderSize = new Point(48, 7);
        var handleSize = new Point(6, 10);

        foreach (var target in targets)
        {
            foreach (var parameter in ParameterScanner.Scan(target))
            {
                ParameterUIFactory.Configure(sliderTexture, panelTexture, selectedTexture, fillTexture, sliderSize, handleSize);
                var element = ParameterUIFactory.Create(parameter);

                if (element != null)
                {
                    InfoPanel.AddElement($"{parameter.Label}_Value", element);
                }
            }
        }
    }

    public virtual void UpdateInfo()
    {
        if (InfoPanel.TryGetElement<TextElement>("pos", out var text))
        {
            text.SetText($"pos: ({transform.position.X}, {transform.position.Y})");
        }
    }

    public virtual bool CanHover(Point mousePos)
    {
        return hitbox.Contains(mousePos);
    }
}