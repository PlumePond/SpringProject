using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Editor;
using SpringProject.Core.UserInput;

namespace SpringProject.Core;

public class GameCamera : Camera
{
    protected Grid _grid;
    protected Transform _target;

    float _speed = 5f;

    public GameCamera(GraphicsDevice graphics, int zoom, Grid grid, Transform target) : base(graphics, zoom)
    {
        _grid = grid;
        _target = target;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_target == null) return;

        Point windowSize = Main.GameWindow.ClientBounds.Size;

        Point scaledSize = new Point(
            windowSize.X / (int)Zoom,
            windowSize.Y / (int)Zoom
        );

        Vector2 targetPosition = new Vector2(_target.position.X - scaledSize.X / 2, _target.position.Y - scaledSize.Y / 2);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Position = Vector2.Lerp(Position, targetPosition, _speed * deltaTime);
    }
}