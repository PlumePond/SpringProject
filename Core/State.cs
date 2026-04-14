using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Editor;

namespace SpringProject.Core;

public abstract class State
{
    protected Entity _entity;
    protected StateMachine _stateMachine;
    protected Animator _animator;

    public State(Entity entity)
    {
        _entity = entity;
        _stateMachine = _entity.StateMachine;
        _animator = _entity.Animator;
    }

    public virtual void Enter()
    {

    }

    public virtual void Update(GameTime gameTime)
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