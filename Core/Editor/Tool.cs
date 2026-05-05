using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.UI;

namespace SpringProject.Core.Editor;

public abstract class Tool(GridPlacement placement)
{
    protected GridPlacement _placement = placement;
    public virtual CursorType CursorType => default; 

    public virtual void Update(GameTime gameTime)
    {
        
    }

    public virtual void PressPrimary(LevelObject hovered, Point worldMousePos, Point uiMousePos, bool swipe)
    {
        
    }

    public virtual void PressSecondary(LevelObject hovered, Point worldMousePos, Point uiMousePos, bool swipe)
    {
        
    }

    public virtual void PressEmpty(Point worldMousePos, Point uiMousePos, bool swipe)
    {
        
    }

    public virtual void Hold(GameTime gameTime, LevelObject hovered)
    {
        
    }

    public virtual void Release()
    {
        
    }

    public virtual void DrawUI(SpriteBatch spriteBatch)
    {
        
    }

    public virtual void DrawWorld(SpriteBatch spriteBatch)
    {
        
    }
}