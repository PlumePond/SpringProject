using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringProject.Core.Editor;
using SpringProject.Core.Components;

namespace SpringProject.Core.Content.Types.LevelObjects;

public class WaterBox : LevelObject
{
    [Parameter("Frame Duration")] public float FrameDuration { get; set; } = 0.1f;

    private float _animTimer = 0f;
    private int _currentFrame = 0;
    private const int FrameCount = 8;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);
        AddComponent<BoxCollider>();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        AdvanceAnimation(gameTime);
    }

    public override void EditorUpdate(GameTime gameTime)
    {
        base.EditorUpdate(gameTime);
        AdvanceAnimation(gameTime);
    }

    void AdvanceAnimation(GameTime gameTime)
    {
        _animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_animTimer >= FrameDuration)
        {
            _animTimer -= FrameDuration;
            _currentFrame = (_currentFrame + 1) % FrameCount;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // --- main body ---
        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Vector2 origin = new Vector2(data.sprite.Width / 2f, data.sprite.Height / 2f);

        SpriteEffects effects = SpriteEffects.None;
        if (transform.flipX) effects |= SpriteEffects.FlipHorizontally;
        if (transform.flipY) effects |= SpriteEffects.FlipVertically;

        Vector2 drawScale = new Vector2((float)size.X / data.sprite.Width, (float)size.Y / data.sprite.Height);

        Color objectColor = selected ? Color.LightGoldenrodYellow * ColorManager.Get(colorIndex) : ColorManager.Get(colorIndex);

        spriteBatch.Draw(data.sprite, drawPos, null, objectColor * tint, 0f, origin, drawScale, effects, 0.15f);

        // --- water top ---
        DrawWaterTop(spriteBatch);

        if (hovered)
            Debug.DrawRectangleOutline(spriteBatch, bounds, Color.White, 1);
        else if (selected)
            Debug.DrawRectangleOutline(spriteBatch, bounds, Color.Yellow, 1);
    }

    void DrawWaterTop(SpriteBatch spriteBatch)
    {
        var topTexture = TextureManager.Get("water_top_scroll");
        var topEdgeTexture = TextureManager.Get("water_top_scroll_edge");
        var frameWidth = topTexture.Width / FrameCount;
        var frameHeight = topTexture.Height;

        Rectangle sourceRect = new Rectangle(_currentFrame * frameWidth, 0, frameWidth, frameHeight);

        // tile the strip across the width of the box
        int tilesNeeded = (int)Math.Ceiling((float)bounds.Width / frameWidth);
        for (int i = 0; i < tilesNeeded; i++)
        {
            int destX = bounds.X + i * frameWidth;
            int destY = bounds.Y - frameHeight; // top edge of the box
            int tileWidth = Math.Min(frameWidth, bounds.Right - destX); // clip the last tile

            Rectangle destRect = new Rectangle(destX, destY, tileWidth, frameHeight);
            Rectangle clippedSource = new Rectangle(sourceRect.X, sourceRect.Y, tileWidth, frameHeight);

            spriteBatch.Draw(topTexture, destRect, clippedSource, ColorManager.Get(colorIndex) * tint, 0f, Vector2.Zero, SpriteEffects.None, 0.14f);
            spriteBatch.Draw(topEdgeTexture, destRect, clippedSource, Color.White * tint, 0f, Vector2.Zero, SpriteEffects.None, 0.13f);
        }
    }

    public override void DrawEditor(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch);
    }

    public override void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        Color hitboxColor = data.solid ? Color.Green : Color.Blue;
        Debug.DrawRectangle(spriteBatch, bounds, hitboxColor * 0.25f);

        string debugText = $"RECTANGLE!!!";
        Vector2 textPos = bounds.Center.ToVector2();
        Vector2 textOrigin = font.FontBase.MeasureString(debugText) * 0.5f;
        spriteBatch.DrawString(font.FontBase, debugText, textPos, Color.White, 0, textOrigin, Vector2.One, 1);
    }
}