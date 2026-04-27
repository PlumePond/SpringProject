using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Components;
using SpringProject.Core.Editor;

namespace SpringProject.Core;

public abstract class State<T> where T : LevelObject
{
    protected T _entity;
    protected StateMachine<T> _stateMachine;
    
    public void Initialize(T entity, StateMachine<T> stateMachine)
    {
        _entity = entity;
        _stateMachine = stateMachine;
    }

    public virtual void Enter()
    {

    }

    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void FixedUpdate(GameTime gameTime)
    {
        
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        
    }

    public virtual void DrawDebug(SpriteBatch spriteBatch)
    {
        
    }

    public virtual void Exit()
    {

    }

    public virtual void IterateFrame(int frame)
    {
        
    }
}