using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Editor;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core;

public class EditorCamera : Camera
{
    protected Grid _grid;

    public EditorCamera(GraphicsDevice graphics, float zoom, Grid grid) : base(graphics, zoom)
    {
        _grid = grid;
    }

    public override void Update(GameTime gameTime)
    {
        // zoom
        // int scrollDelta = Input.Get("camera_zoom").DeltaInt;

        // if (scrollDelta != 0)
        // {
        //     Vector2 mouseScreen = Input.Get("cursor").Vector;
        //     Vector2 mouseWorldBefore = ScreenToWorld(mouseScreen);

        //     // step zoom by whole integers, clamped to [1, 10]
        //     Zoom = MathHelper.Clamp(Zoom + (scrollDelta > 0 ? 1f : -1f), 1f, 10f);

        //     UpdateTransform();

        //     Vector2 mouseWorldAfter = ScreenToWorld(mouseScreen);
        //     Position += mouseWorldBefore - mouseWorldAfter;
        // }

        // middle mouse drag for panning
        if (Input.Get("camera_pan").Holding)
        {
            Vector2 mouseDelta = Input.Get("cursor").DeltaVector;
            Position -= mouseDelta / Zoom;
        }

        if (Input.Get("camera_pan").Pressed)
        {
            Cursor.BeginGrab();
        }

        if (Input.Get("camera_pan").Released)
        {
            Cursor.EndGrab();
        }

        base.Update(gameTime);
    }
}