using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Audio;
using SpringProject.Core.Content.Types.LevelObjects;
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

        if (Player.Instance == null) return;

        Point windowSize = Main.GameWindow.ClientBounds.Size;

        Point scaledSize = new Point(
            windowSize.X / (int)Zoom,
            windowSize.Y / (int)Zoom
        );
        var target = Player.Instance.transform;
        Vector2 targetPosition = new Vector2(target.position.X - scaledSize.X / 2, target.position.Y - scaledSize.Y / 2);
        Position = targetPosition;
    }

    public override void Update(GameTime gameTime)
    {
        //zoom
        int scrollDelta = Input.Get("camera_zoom").DeltaInt;

        if (scrollDelta != 0 && !Input.MouseHoverConsumed)
        {
            Vector2 mouseScreen = Input.Get("cursor").Vector;
            Vector2 mouseWorldBefore = ScreenToWorld(mouseScreen);

            // step zoom by whole integers, clamped to [1, 10]
            Zoom = MathHelper.Clamp(Zoom + (scrollDelta > 0 ? 1f : -1f), 1f, 4f);

            UpdateTransform();

            Vector2 mouseWorldAfter = ScreenToWorld(mouseScreen);
            Position += mouseWorldBefore - mouseWorldAfter;
        }

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
        
        AudioManager.SetListenerPosition(Position + new Vector2(Main.GameWindow.ClientBounds.Width, Main.GameWindow.ClientBounds.Height) / (2 * Zoom));

        base.Update(gameTime);
    }
}