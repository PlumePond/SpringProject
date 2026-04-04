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
using SpringProject.Core.Audio;

namespace SpringProject.Core.Content.Types.LevelObjects;

public class Pot : Entity
{
    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        Animator.Add("default", new Animation(0, 1, 0.1f, false));
        Animator.Add("wiggle", new Animation(1, 5, 0.1f, false));

        Animator.Set("default");
    }

    public override void OnEntityEnter(Entity other)
    {
        Animator.Set("wiggle");
        AudioManager.Get("pot_touch").Play();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void DrawDebug(SpriteBatch spriteBatch, SpriteFontBase font)
    {
        base.DrawDebug(spriteBatch, font);

        // string debugText = $"{_position}";
        // Vector2 textPos = bounds.Center.ToVector2();
        // Vector2 textOrigin = font.MeasureString(debugText) * 0.5f;
        // spriteBatch.DrawString(font, debugText, textPos, Color.White, 0, textOrigin, Vector2.One * 0.25f);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
}