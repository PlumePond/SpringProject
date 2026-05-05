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

public class Box : LevelObject
{
    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);
        AddComponent<BoxCollider>();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Vector2 origin = new Vector2(data.sprite.Width / 2f, data.sprite.Height / 2f);
        float radians = transform.rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (transform.flipX) effects |= SpriteEffects.FlipHorizontally;
        if (transform.flipY) effects |= SpriteEffects.FlipVertically;

        Vector2 drawScale = new Vector2((float)size.X / data.sprite.Width, (float)size.Y / data.sprite.Height);

        Color objectColor = selected ? Color.LightGoldenrodYellow * ColorManager.Get(colorIndex) : ColorManager.Get(colorIndex);

        spriteBatch.Draw(data.sprite, drawPos, null, objectColor * tint, radians, origin, drawScale, effects, 0f);

        if (hovered)
        {
            Debug.DrawRectangleOutline(spriteBatch, bounds, Color.White, 1);
        }
        else if (selected)
        {
            Debug.DrawRectangleOutline(spriteBatch, bounds, Color.Yellow, 1);
        }
    }

    public override void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        Color hitboxColor = data.solid ? Color.Green : Color.Blue;
        Debug.DrawRectangle(spriteBatch, bounds, hitboxColor * 0.25f);

        string debugText = $"RECTANGLE!!!";
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
}