using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.UserInput;

namespace SpringProject.Core;

public class Camera
{
    public Matrix Transform { get; private set; }
    public Vector2 Position { get; protected set; }
    public float Zoom { get; protected set; } = 1f;
    protected GraphicsDevice _graphics;
    public static Camera Instance;

    public Camera(GraphicsDevice graphics, float zoom)
    {
        _graphics = graphics;
        Zoom = zoom;
        Instance = this;
    }

    public virtual void Update(GameTime gameTime)
    {
        UpdateTransform();
    }

    public Matrix GetParallaxTransform(float parallaxFactor)
    {
        float multiplier = 1f - parallaxFactor;
        return Matrix.CreateTranslation(new Vector3(-Position.X * multiplier, -Position.Y * multiplier, 0)) *
            Matrix.CreateScale(Zoom, Zoom, 1f);
    }

    protected void UpdateTransform()
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

    public Vector2 ScreenToWorld(Vector2 screenPos, float parallaxFactor)
    {
        Matrix layerTransform = GetParallaxTransform(parallaxFactor);
        return Vector2.Transform(screenPos, Matrix.Invert(layerTransform));
    }

    public Vector2 WorldToScreen(Vector2 worldPos, float parallaxFactor)
    {
        Matrix layerTransform = GetParallaxTransform(parallaxFactor);
        return Vector2.Transform(worldPos, layerTransform);
    }
}