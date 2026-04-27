using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Components;

public abstract class Component
{
    public LevelObject LevelObject { get; internal set; }
    public bool Enabled { get; set; } = true;

    public Component()
    {
        ComponentSystem.Add(this);
    }

    public virtual void Start() {}
    public virtual void Update(GameTime gameTime) {}
    public virtual void FixedUpdate(GameTime gameTime) {}
    public virtual void EditorUpdate(GameTime gameTime) {}
    public virtual void Draw(SpriteBatch spriteBatch) {}
    public virtual void DrawDebug(SpriteBatch spriteBatch) {}
    public virtual void OnDestroy()
    {
        ComponentSystem.Remove(this);
    }
}