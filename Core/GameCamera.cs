using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Audio;
using SpringProject.Core.Editor;

namespace SpringProject.Core;

public class GameCamera : Camera
{
    protected Grid _grid;
    protected Transform _target;

    const float FOLLOW_SPEED = 5f;

    readonly Stack<Rectangle> _boundsStack = new();

    Rectangle _bounds;

    public static GameCamera Instance;

    public GameCamera(GraphicsDevice graphics, int zoom, Grid grid, Transform target) : base(graphics, zoom)
    {
        _grid = grid;
        _target = target;

        var root = new Rectangle(0, 0, grid.size.X * grid.GridSize, grid.size.Y * grid.GridSize);
        _boundsStack.Push(root);

        Bounds = root;

        Point windowSize = Main.GameWindow.ClientBounds.Size;
        Point scaledSize = new(windowSize.X / (int)Zoom, windowSize.Y / (int)Zoom);

        Vector2 startPos = new(target.position.X - scaledSize.X / 2f, target.position.Y - scaledSize.Y / 2f);
        startPos.X = MathHelper.Clamp(startPos.X, root.Left, Math.Max(root.Left, root.Right - scaledSize.X));
        startPos.Y = MathHelper.Clamp(startPos.Y, root.Top, Math.Max(root.Top, root.Bottom - scaledSize.Y));
        Position = startPos;

        Instance = this;
    }

    public void PushBounds(Rectangle added)
    {
        _boundsStack.Push(added);
        _bounds = added;
    }

    public void PopBounds(Rectangle removed)
    {
        var items = _boundsStack.ToArray();
        _boundsStack.Clear();

        foreach (var item in items.Reverse())
        {
            if (item != removed)
            {
                _boundsStack.Push(item);
            }
        }

        _bounds = _boundsStack.Count > 0 ? _boundsStack.Peek() : new Rectangle(0, 0, _grid.size.X * _grid.GridSize, _grid.size.Y * _grid.GridSize);
    }

    public override void Update(GameTime gameTime)
    {
        if (_target == null) return;

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Point windowSize = Main.GameWindow.ClientBounds.Size;
        Point scaledSize = new(windowSize.X / (int)Zoom, windowSize.Y / (int)Zoom);

        Vector2 targetPos = new(_target.position.X - scaledSize.X / 2f, _target.position.Y - scaledSize.Y / 2f);

        targetPos.X = MathHelper.Clamp(targetPos.X, _bounds.Left, Math.Max(_bounds.Left, _bounds.Right - scaledSize.X));
        targetPos.Y = MathHelper.Clamp(targetPos.Y, _bounds.Top, Math.Max(_bounds.Top, _bounds.Bottom - scaledSize.Y));

        Position = Vector2.Lerp(Position, targetPos, FOLLOW_SPEED * deltaTime);

        AudioManager.SetListenerPosition(Position + new Vector2(Main.GameWindow.ClientBounds.Width, Main.GameWindow.ClientBounds.Height) / (2 * Zoom));

        base.Update(gameTime);
    }
}