using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core;

public class Camera
{
    public Matrix Transform { get; private set; }
    public Vector2 Position = Vector2.Zero;
    public float Zoom = 1f;
    private MouseState _prevMouse;
    private GraphicsDevice _graphics;

    public Camera(GraphicsDevice graphics)
    {
        _graphics = graphics;
    }

    public void Update()
    {
        MouseState mouse = Mouse.GetState();
        KeyboardState keyboard = Keyboard.GetState();

        // Handle zoom with scroll wheel
        int scrollDelta = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;

        if (scrollDelta != 0 && !keyboard.IsKeyDown(Keys.LeftControl))
        {
            Vector2 mouseScreen = new Vector2(mouse.X, mouse.Y);
            Vector2 mouseWorldBefore = ScreenToWorld(mouseScreen);

            // Step zoom by whole integers, clamped to [1, 10]
            Zoom = MathHelper.Clamp(Zoom + (scrollDelta > 0 ? 1f : -1f), 1f, 10f);

            UpdateTransform();

            Vector2 mouseWorldAfter = ScreenToWorld(mouseScreen);
            Position += mouseWorldBefore - mouseWorldAfter;
        }

        // Middle mouse drag for panning
        if (mouse.MiddleButton == ButtonState.Pressed && _prevMouse.MiddleButton == ButtonState.Pressed)
        {
            Vector2 mouseDelta = new Vector2(mouse.X - _prevMouse.X, mouse.Y - _prevMouse.Y);
            Position -= mouseDelta / Zoom;
        }

        UpdateTransform();

        _prevMouse = mouse;
    }

    private void UpdateTransform()
    {
        Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                    Matrix.CreateScale(Zoom, Zoom, 1f);
    }

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return Vector2.Transform(screenPos, Matrix.Invert(Transform));
    }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        return Vector2.Transform(worldPos, Transform);
    }
}