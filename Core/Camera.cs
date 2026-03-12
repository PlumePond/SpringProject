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
            // Get mouse position in screen coordinates
            Vector2 mouseScreen = new Vector2(mouse.X, mouse.Y);

            // Convert mouse position to world coordinates BEFORE zoom change
            Vector2 mouseWorldBefore = ScreenToWorld(mouseScreen);

            // Apply zoom change
            float zoomFactor = scrollDelta > 0 ? 1.1f : 0.9f;
            Zoom *= zoomFactor;
            Zoom = MathHelper.Clamp(Zoom, 0.1f, 10f);

            // Update the transform matrix with new zoom
            UpdateTransform();

            // Convert mouse position to world coordinates AFTER zoom change
            Vector2 mouseWorldAfter = ScreenToWorld(mouseScreen);

            // Calculate the difference and adjust camera position
            Vector2 worldDelta = mouseWorldBefore - mouseWorldAfter;
            Position += worldDelta;
        }

        // Middle mouse drag for panning
        if (mouse.MiddleButton == ButtonState.Pressed && _prevMouse.MiddleButton == ButtonState.Pressed)
        {
            Vector2 mouseDelta = new Vector2(mouse.X - _prevMouse.X, mouse.Y - _prevMouse.Y);
            Position -= mouseDelta / Zoom;
        }

        // Update transformation matrix
        UpdateTransform();

        _prevMouse = mouse;
    }

    private void UpdateTransform()
    {
        // Create the transform matrix
        // Note: We need to account for the screen center offset
        Vector2 screenCenter = new Vector2(_graphics.Viewport.Width / 2f, _graphics.Viewport.Height / 2f);

        Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                   Matrix.CreateScale(Zoom, Zoom, 1f);
                   //Matrix.CreateTranslation(new Vector3(screenCenter.X, screenCenter.Y, 0));
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