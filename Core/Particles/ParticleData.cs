using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;

namespace SpringProject.Core.Particles;

public struct ParticleData
{
    public Texture2D texture = TextureManager.Get("particle_dust_big");
    public Vector2 direction = new Vector2(0.0f, -1.0f);
    public float lifespan = 1.0f;

    public Color startColor = Color.White;
    public Color endColor = Color.White;
    public float startOpacity = 1.0f;
    public float endOpacity = 0.0f;
    public float startSpeed = 0.0f;
    public float endSpeed = 0.0f;
    public float startRotation = 0.0f;
    public float endRotation = 0.0f;

    public int frameCount = 0;
    public float frameInterval = 0;
    public Point frameSize = Point.Zero;
    public bool loop = false;

    public Vector2 gravity = Vector2.Zero;

    public ParticleData()
    {
        
    }
}