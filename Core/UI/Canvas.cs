using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UI;

public class Canvas : Element
{
    public Canvas(Point localPosition, Point size, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, size, anchor)
    {
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Main.UIMatrtix);

        DrawChildren(spriteBatch);

        spriteBatch.End();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        UpdateChildren(gameTime);
    }
}