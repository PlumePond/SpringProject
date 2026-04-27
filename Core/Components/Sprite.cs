using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Components;
using SpringProject.Core.Editor;

namespace SpringProject.Core;

public class Sprite : Component
{
    Rectangle _sourceRectOverride;
    bool _overrideSourceRect;

    public override void Draw(SpriteBatch spriteBatch)
    {
        Point frame = LevelObject.data.frame;
        Point size = LevelObject.data.size;
        Rectangle bounds = LevelObject.bounds;
        Point defaultFramePos = LevelObject.data.defaultFramePos;
        int colorIndex = LevelObject.colorIndex;
        Texture2D sprite = LevelObject.data.sprite;

        Point framedSize = frame != Point.Zero ? frame : size;
        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Vector2 origin = new Vector2(framedSize.X / 2f, framedSize.Y / 2f);
        float radians = LevelObject.transform.rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (LevelObject.transform.flipX) effects |= SpriteEffects.FlipHorizontally;
        if (LevelObject.transform.flipY) effects |= SpriteEffects.FlipVertically;

        Vector2 drawScale = new Vector2((float)size.X / sprite.Width, (float)size.Y / sprite.Height);
        Color objectColor = LevelObject.selected ? Color.LightGoldenrodYellow * ColorManager.Get(colorIndex) : ColorManager.Get(colorIndex);

        Rectangle? sourceRect;
        
        if (_overrideSourceRect)
        {
            sourceRect = _sourceRectOverride;
        }
        else
        {
            sourceRect = frame != Point.Zero ? new Rectangle(defaultFramePos, frame) : null;
        }

        spriteBatch.Draw(sprite, drawPos, sourceRect, objectColor * LevelObject.tint, radians, origin, drawScale, effects, 0);
    }

    public void SetSourceRect(Rectangle rectangle)
    {
        if (!_overrideSourceRect)
        {
            _overrideSourceRect = true;
        }
        _sourceRectOverride = rectangle;
    }
}