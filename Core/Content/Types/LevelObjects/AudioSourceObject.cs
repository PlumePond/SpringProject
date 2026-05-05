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
using SpringProject.Core.Audio;

namespace SpringProject.Core.Content.Types.LevelObjects;

public class AudioSourceObject : LevelObject
{
    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        AddComponent<AudioSourceComponent>();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // don't draw anything
    }

    public override void DrawEditor(SpriteBatch spriteBatch)
    {
        base.DrawEditor(spriteBatch);
    }
}